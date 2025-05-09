using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ManualUpdate;

namespace Pawnshop.Data.Access
{
    public class ManualUpdateExecuteRepository : RepositoryBase
    {
        public ManualUpdateExecuteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public string GetDynamic(ManualUpdateRequest entity)
        {
            dynamic row = UnitOfWork.Session.Query<dynamic>(entity.SelectQuery, new { entity }, UnitOfWork.Transaction);

            Regex regex = new Regex(@"DapperRow, ");
            return regex.Replace(string.Join(", ", row), "");
        }

        public async Task Insert(ManualUpdateRequest entity)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(entity.UpdateQuery, new { entity }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}