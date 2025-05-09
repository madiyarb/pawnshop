using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ApplicationOnlineRejectionReasons;
using Pawnshop.Data.Models.ApplicationOnlineRejectionReasons.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineRejectionReasonsRepository : RepositoryBase
    {
        public ApplicationOnlineRejectionReasonsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<ApplicationOnlineRejectionReason> Get(int id)
        {
            return await UnitOfWork.Session.GetAsync<ApplicationOnlineRejectionReason>(id);
        }

        public async Task<ApplicationOnlineRejectionReasonListView> GetFiltredRejectionReasons(int offset = 0, int limit = int.MaxValue,
            bool? forClient = null, bool? forManager = null, string? code = null, bool? enabled = null)
        {
            ApplicationOnlineRejectionReasonListView listView = new ApplicationOnlineRejectionReasonListView();
            var builder = new SqlBuilder();

            if (enabled != null)
            {
                builder.Where("Enabled = @enabled", new { enabled });
            }

            if (!string.IsNullOrEmpty(code))
            {
                builder.Where("code = @code", new { code });
            }
            if (forClient != null)
            {
                builder.Where("availableToChoiceForClient = @availableToChoiceForClient",
                    new { availableToChoiceForClient = forClient });
            }

            if (forManager != null)
            {
                builder.Where("AvailableToChoiceForManager = @AvailableToChoiceForManager",
                    new { AvailableToChoiceForManager = forManager });
            }

            builder.OrderBy("id");
            var selector = builder.AddTemplate(@$"SELECT * from ApplicationOnlineRejectionReasons /**where**/ /**orderby**/ 
            OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var countTemplate = builder.AddTemplate(
                @$"SELECT COUNT(*) from ApplicationOnlineRejectionReasons /**where**/");
            var count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters)).FirstOrDefault();
            return new ApplicationOnlineRejectionReasonListView
            {
                Count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters))
                    .FirstOrDefault(),
                List = (await UnitOfWork.Session.QueryAsync<ApplicationOnlineRejectionReason>(selector.RawSql,
                    selector.Parameters)).ToList()
            };
        }

        public async Task<List<ApplicationOnlineRejectionReason>> GetAll()
        {
            return (await UnitOfWork.Session.GetAllAsync<ApplicationOnlineRejectionReason>()).ToList();
        }

        public async Task<int> Insert(ApplicationOnlineRejectionReason rejectionReason)
        {
            return await UnitOfWork.Session.InsertAsync(rejectionReason);
        }

        public async Task<bool> Update(ApplicationOnlineRejectionReason rejectionReason)
        {
            return await UnitOfWork.Session.UpdateAsync(rejectionReason);
        }

        public async Task<ApplicationOnlineRejectionReason> FindByCode(string code)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ApplicationOnlineRejectionReason>(@"SELECT * FROM ApplicationOnlineRejectionReasons WHERE Code = @code",
                new { code }, UnitOfWork.Transaction);
        }
    }
}
