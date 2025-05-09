using Dapper.Contrib.Extensions;
using Dapper;
using Microsoft.AspNetCore.Http;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Access.ApplicationOnlineHistoryLogger;
using Pawnshop.Data.Helpers;
using Pawnshop.Data.Models.ApplicationOnlineFileCodes;
using Pawnshop.Data.Models.ApplicationOnlineFileLogItems;
using Pawnshop.Data.Models.ApplicationOnlineFiles.Views;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineFileRepository : RepositoryBase
    {
        private readonly IApplicationOnlineHistoryLoggerService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationOnlineFileRepository(IUnitOfWork unitOfWork, IApplicationOnlineHistoryLoggerService service,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApplicationOnlineFile> Get(Guid id)
        {
            var builder = new SqlBuilder();
            builder.Select("ApplicationOnlineFiles.*");
            builder.Where("ApplicationOnlineFiles.DeleteDate is NULL");
            builder.Where("ApplicationOnlineFiles.id = @id", new { id = id });
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ApplicationOnlineFiles  /**where**/ ");

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<ApplicationOnlineFile>(builderTemplate.RawSql, builderTemplate.Parameters);
        }

        public async Task Insert(ApplicationOnlineFile applicationOnlineFile)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(applicationOnlineFile, UnitOfWork.Transaction);
                transaction.Commit();
            }

            await _service.LogApplicationOnlineFileData(new ApplicationOnlineFileLogData(applicationOnlineFile),
                _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
        }

        public async Task Update(ApplicationOnlineFile applicationOnlineFile)
        {
            using (var transaction = BeginTransaction())
            {
                var result = await UnitOfWork.Session.UpdateAsync(applicationOnlineFile, UnitOfWork.Transaction);
                transaction.Commit();
            }
            await _service.LogApplicationOnlineFileData(new ApplicationOnlineFileLogData(applicationOnlineFile),
                _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
        }

        public async Task<IEnumerable<ApplicationOnlineFile>> GetApplicationOnlineFilesForEstimation(Guid applicationId)
        {
            var builder = new SqlBuilder();
            builder.Select("ApplicationOnlineFiles.*");

            builder.Join(
                "ApplicationOnlineFileCodes ON  ApplicationOnlineFileCodes.Id = ApplicationOnlineFiles.ApplicationOnlineFileCodeId");
            builder.Where("ApplicationId = @applicationId", new { applicationId = applicationId });
            builder.Where("ApplicationOnlineFileCodes.Category in ('CarPhoto' , 'ClientDocument', 'CarDocument')");
            builder.Where("ApplicationOnlineFiles.SendToEstimate = 1");
            builder.Where("ApplicationOnlineFiles.DeleteDate is null");
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ApplicationOnlineFiles /**join**/  /**where**/ ");

            return await UnitOfWork.Session.QueryAsync<ApplicationOnlineFile>(builderTemplate.RawSql,
                builderTemplate.Parameters);
        }

        public async Task<IEnumerable<ApplicationOnlineFile>> GetFilesForEstimation(Guid applicationId)
        {
            return await UnitOfWork.Session.QueryAsync<ApplicationOnlineFile, ApplicationOnlineFileCode, ApplicationOnlineFile>(@"SELECT aof.*,
       aofc.*
  FROM ApplicationOnlineFiles aof
  JOIN ApplicationOnlineFileCodes aofc ON aofc.Id = aof.ApplicationOnlineFileCodeId
 WHERE ApplicationId = @applicationId
   AND aofc.Category IN ('CarPhoto' , 'ClientDocument', 'CarDocument')
   AND aof.SendToEstimate = 1
   AND aof.DeleteDate IS NULL",
                (file, codeInfo) =>
                {
                    file.ApplicationOnlineFileCode = codeInfo;

                    return file;
                },
                new { applicationId }, UnitOfWork.Transaction);
        }

        public async Task<ApplicationOnlineFileListView> GetListView(Guid applicationId, int offset, int limit)
        {
            var builder = new SqlBuilder();
            builder.Select("ApplicationOnlineFiles.*");
            builder.Select("ApplicationOnlineFileCodes.BusinessType");
            builder.Select("ApplicationOnlineFileCodes.Code");
            builder.Select("ApplicationOnlineFileCodes.Title");
            builder.Select("ApplicationOnlineFileCodes.StorageFileTitle");
            builder.Select("ApplicationOnlineFileCodes.Category");
            builder.LeftJoin(
                "ApplicationOnlineFileCodes ON  ApplicationOnlineFileCodes.Id = ApplicationOnlineFiles.ApplicationOnlineFileCodeId");
            builder.Where("ApplicationOnlineFiles.ApplicationId = @applicationId", new { applicationId });
            builder.Where("ApplicationOnlineFiles.DeleteDate is null");
            builder.OrderBy("UpdateDate");
            var selector = builder.AddTemplate($"Select /**select**/ from ApplicationOnlineFiles /**leftjoin**/ /**where**/ /**orderby**/ " +
                                                      $"OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var counter =
                builder.AddTemplate(
                    $"Select count(*) from ApplicationOnlineFiles /**leftjoin**/ /**where**/");

            ApplicationOnlineFileListView listView = new ApplicationOnlineFileListView();

            listView.Count = await UnitOfWork.Session.QuerySingleAsync<int>(counter.RawSql,
                counter.Parameters);

            if (listView.Count == 0)
                return null;

            listView.Files = (await UnitOfWork.Session.QueryAsync<ApplicationOnlineFileView>(selector.RawSql,
                selector.Parameters)).ToList();

            return listView;
        }

        public List<ApplicationOnlineFileFromMobile> GetListFromMobile(Guid applicationId)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineFileFromMobile>(@"SELECT aof.Id
       aofc.Title AS Title,
       aofc.BusinessType AS Code,
       aof.StorageFileId AS FileGuid,
       aof.CreateDate AS CreateDate
  FROM ApplicationOnlineFiles aof 
  JOIN ApplicationOnlineFileCodes aofc ON aofc.Id = aof.ApplicationOnlineFileCodeId
 WHERE aof.ApplicationId = @applicationId",
                new { applicationId }, UnitOfWork.Transaction)
                .ToList();
        }

        public ApplicationOnlineFileFromMobile GetLoanContractFileFromMobile(Guid applicationId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ApplicationOnlineFileFromMobile>(@"SELECT TOP 1 aof.Id,
       aofc.Title AS Title,
       aofc.BusinessType AS Code,
       CAST(aof.StorageFileId AS NVARCHAR(36)) AS FileGuid,
       aof.CreateDate AS CreateDate
  FROM ApplicationOnlineFiles aof 
  JOIN ApplicationOnlineFileCodes aofc ON aofc.Id = aof.ApplicationOnlineFileCodeId
 WHERE aof.ApplicationId = @applicationId
   AND aofc.BusinessType = @businessType
   AND aof.DeleteDate is null
 ORDER BY aof.CreateDate DESC",
                new { applicationId, businessType = Constants.APPLICATION_ONLINE_FILE_BUSINESS_TYPE_LOAN_CONTRACT }, UnitOfWork.Transaction);
        }

        public List<ApplicationOnlineFile> GetList(Guid applicationId, List<Guid> fileCodeIds)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineFile>(@"SELECT *
  FROM ApplicationOnlineFiles
 WHERE ApplicationId = @applicationId
   AND ApplicationOnlineFileCodeId IN @fileCodeIds
   AND DeleteDate IS NULL",
                new { applicationId, fileCodeIds }, UnitOfWork.Transaction)
                .ToList();
        }
    }
}
