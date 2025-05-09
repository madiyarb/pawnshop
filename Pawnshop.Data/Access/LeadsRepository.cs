using Pawnshop.Core;
using Pawnshop.Core.Impl;
using System;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Pawnshop.Data.Models.Leads;
using Dapper;

namespace Pawnshop.Data.Access
{
    public sealed class LeadsRepository : RepositoryBase
    {
        public LeadsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public async Task<Lead> Get(Guid id)
        {
            return await UnitOfWork.Session.GetAsync<Lead>(id);
        }

        public async Task<Lead> GetByNumber(string number)
        {
            var builder = new SqlBuilder();
            builder.Select("Leads.*");
            builder.Where("Leads.Phone = @number", new {number = number});
            var selector = builder.AddTemplate($"Select /**select**/ from Leads /**where**/");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Lead>(selector.RawSql,
                selector.Parameters);
        }

        public async Task Insert(Lead lead)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(lead, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}
