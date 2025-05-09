using Dapper.Contrib.Extensions;
using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineFileCodes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineFileCodesRepository : RepositoryBase
    {
        public ApplicationOnlineFileCodesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public List<ApplicationOnlineFileCode> GetApplicationOnlineFileCodes()
        {
            return UnitOfWork.Session.Query<ApplicationOnlineFileCode>(@"Select * from ApplicationOnlineFileCodes",
                UnitOfWork.Transaction).ToList();
        }

        public ApplicationOnlineFileCode GetApplicationOnlineFileCodeByCode(string code)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineFileCode>(@"Select * from ApplicationOnlineFileCodes where Code = @code",
                new { code }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ApplicationOnlineFileCode GetApplicationOnlineFileCodeByBusinessType(string BusinessType)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineFileCode>(@"Select * from ApplicationOnlineFileCodes where BusinessType = @BusinessType",
                new { BusinessType }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ApplicationOnlineFileCode GetApplicationOnlineFileCodeByBusinessType(string BusinessType, string language)
        {
            var entity = UnitOfWork.Session.Query<ApplicationOnlineFileCode>(@"Select * from ApplicationOnlineFileCodes where BusinessType = @BusinessType",
                new { BusinessType }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity != null && entity.NpckTitleId.HasValue)
            {
                if (string.IsNullOrEmpty(language))
                    language = "ru";

                entity.NpckTitle = UnitOfWork.Session.QueryFirstOrDefault<string>(@"SELECT VALUE
  FROM Localizations
 WHERE LocalizationItemId = @npckTitleId
  AND Language = @language",
                    new { npckTitleId = entity.NpckTitleId, language }, UnitOfWork.Transaction);
            }

            return entity;
        }

        public void Insert(ApplicationOnlineFileCode applicationOnlineFileCode)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(@"
	            INSERT INTO ApplicationOnlineFileCodes (Id, BusinessType, Code, Title, StorageFileTitle, Category, EstimationServiceCodeId, NpckTitleId)
	            VALUES (@Id, @BusinessType, @Code, @Title, @StorageFileTitle, @Category, @EstimationServiceCodeId, @NpckTitleId);
                SELECT SCOPE_IDENTITY()", applicationOnlineFileCode, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<List<ApplicationOnlineFileCode>> GetAll()
        {
            return (await UnitOfWork.Session.GetAllAsync<ApplicationOnlineFileCode>()).ToList();
        }
    }
}
