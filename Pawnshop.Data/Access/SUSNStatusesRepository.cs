using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.SUSNStatuses;
using Pawnshop.Data.Models.SUSNStatuses.Views;

namespace Pawnshop.Services.SUSNStatuses
{
    public sealed class SUSNStatusesRepository : RepositoryBase
    {
        public SUSNStatusesRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public async Task<SUSNStatus> Get(int id)
        {
            return await UnitOfWork.Session.GetAsync<SUSNStatus>(id);
        }

        public async Task<IEnumerable<SUSNStatus>> GetAll()
        {
            return await UnitOfWork.Session.GetAllAsync<SUSNStatus>();
        }

        public async Task Insert(SUSNStatus status)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(status, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task Update(SUSNStatus status)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.UpdateAsync(status, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<SUSNStatusListView> GetListView(int offset, int limit)
        {
            SUSNStatusListView listView = new SUSNStatusListView();
            var builder = new SqlBuilder();
            builder.Select("SUSNStatuses.*");
            builder.OrderBy("SUSNStatuses.Id");
            builder.Where("SUSNStatuses.DeleteDate is null");

            var selector = builder.AddTemplate($"Select /**select**/ from SUSNStatuses /**where**/ /**orderby**/ " +
                                               $"OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var counter =
                builder.AddTemplate(
                    $"Select count(*) from SUSNStatuses /**where**/");

            listView.Count = await UnitOfWork.Session.QuerySingleAsync<int>(counter.RawSql,
                counter.Parameters);

            if (listView.Count == 0)
                return null;

            listView.List = (await UnitOfWork.Session.QueryAsync<SUSNStatusView>(selector.RawSql,
                selector.Parameters)).ToList();

            return listView;
        }

        public async Task<SUSNStatus> GetByCode(string code)
        {
            var builder = new SqlBuilder();
            builder.Select("SUSNStatuses.*");
            builder.Where("SUSNStatuses.DeleteDate is null");
            builder.Where("SUSNStatuses.Code =@code", new { code });
            var selector = builder.AddTemplate($"Select /**select**/ from SUSNStatuses /**where**/ ");
            return (await UnitOfWork.Session.QueryAsync<SUSNStatus>(selector.RawSql,
                selector.Parameters)).FirstOrDefault();
        }
    }
}
