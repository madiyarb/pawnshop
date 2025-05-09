using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.BranchesPartnerCodes;
using Pawnshop.Data.Models.BranchesPartnerCodes.Query;
using Pawnshop.Data.Models.BranchesPartnerCodes.Views;

namespace Pawnshop.Data.Access
{
    public sealed class BranchesPartnerCodesRepository : RepositoryBase
    {
        public BranchesPartnerCodesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public async Task Insert(BranchesPartnerCode entity)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<BranchesPartnerCode> Get(Guid id)
        {
            return await UnitOfWork.Session.GetAsync<BranchesPartnerCode>(id);
        }

        public async Task Update(BranchesPartnerCode entity)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.UpdateAsync(entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<BranchPartnerCodeListView> GetListView(BranchPartnerCodeListQuery query, int offset, int limit)
        {
            BranchPartnerCodeListView listView = new BranchPartnerCodeListView();

            SqlBuilder builder = new SqlBuilder();

            #region Select
            builder.Select(@"BranchesPartnerCodes.*");
            builder.Select(@"Groups.DisplayName AS BranchName");
            #endregion

            #region Join
            builder.Join(@"Groups ON Groups.Id = BranchesPartnerCodes.BranchId");
            #endregion

            #region Where
            if (query.BranchId.HasValue)
            {
                builder.Where("BranchesPartnerCodes.BranchId = @BranchId", new { BranchId = query.BranchId });
            }
            if (query.Enabled.HasValue)
            {
                builder.Where("BranchesPartnerCodes.Enabled = @Enabled", new { Enabled = query.Enabled });
            }
            if (query.Id.HasValue)
            {
                builder.Where("BranchesPartnerCodes.Id = @Id", new { Id = query.Id });
            }
            if (!string.IsNullOrEmpty(query.PartnerCode))
            {
                builder.Where("BranchesPartnerCodes.PartnerCode = @PartnerCode", new { PartnerCode = query.PartnerCode });
            }

            builder.Where("BranchesPartnerCodes.DeleteDate is null");
            #endregion

            #region OrderBy

            builder.OrderBy("BranchesPartnerCodes.CreateDate Desc");

            #endregion

            var selector = builder.AddTemplate($@"Select /**select**/ from BranchesPartnerCodes 
            /**join**/ /**where**/ /**orderby**/ OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var counter = builder.AddTemplate($@"Select count(*) from BranchesPartnerCodes 
            /**join**/ /**where**/");

            listView.Count = await UnitOfWork.Session.QueryFirstAsync<int>(counter.RawSql, selector.Parameters);
            listView.Items = await UnitOfWork.Session.QueryAsync<BranchPartnerCodeView>(selector.RawSql, selector.Parameters);

            return listView;
        }
    }
}
