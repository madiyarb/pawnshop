using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using AccrualBase = Pawnshop.Data.Models.AccountingCore.AccrualBase;

namespace Pawnshop.Data.Access.AccountingCore
{
    public class AccrualBaseRepository : RepositoryBase, IRepository<AccrualBase>
    {
        public AccrualBaseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AccrualBase entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO AccrualBases (AccrualType, BaseSettingId, AmountType, Name, NameAlt, AuthorId, IsActive, CreateDate, RateSettingId  )
VALUES (@AccrualType, @BaseSettingId, @AmountType, @Name, @NameAlt, @AuthorId, @IsActive, @CreateDate, @RateSettingId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(AccrualBase entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
UPDATE AccrualBases
SET AccrualType = @AccrualType, BaseSettingId = @BaseSettingId, AmountType = @AmountType, Name = @Name, NameAlt = @NameAlt, IsActive = @IsActive, RateSettingId = @RateSettingId
WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public AccrualBase Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AccrualBase>(@"
SELECT *
FROM AccrualBases
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public async Task<AccrualBase> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<AccrualBase>(@"
SELECT *
FROM AccrualBases
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public AccrualBase Find(object query)
        {
            throw new NotImplementedException();
        }

		public List<AccrualBase> List(ListQuery listQuery, object query = null)
		{
			if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

			List<string> conditions = new List<string>{};

            bool? isActive = query?.Val<bool?>("IsActive");
            int? baseSettingId = query?.Val<int?>("BaseSettingId");
            AccrualType? accrualType = query?.Val<AccrualType?>("AccrualType");
            AmountType? amountType = query?.Val<AmountType?>("AmountType");
            ContractClass? contractClass = query?.Val<ContractClass?>("ContractClass");

            if (baseSettingId.HasValue)
                conditions.Add("ab.BaseSettingId = @baseSettingId");
            if (isActive.HasValue)
                conditions.Add("ab.IsActive = @isActive");
            if (accrualType.HasValue)
                conditions.Add("ab.AccrualType = @accrualType");
            if (amountType.HasValue)
                conditions.Add("ab.AmountType = @amountType");
            if (contractClass.HasValue)
                conditions.Add("s.ContractClass = @contractClass");

	        conditions.Add("r.DeleteDate is null");

			string condition = string.Empty;

			if (conditions.Count > 0)
			{
				condition = "WHERE " + string.Join(" AND ", conditions);
			}

			var order = listQuery.Order(string.Empty, new Sort
			{
				Name = "ab.AccrualType",
				Direction = SortDirection.Asc
			});
			var page = listQuery.Page();

            return UnitOfWork.Session.Query<AccrualBase>($@"
SELECT distinct ab.*
FROM AccrualBases ab
JOIN LoanSettingRates r on r.[RateSettingId] = ab.RateSettingId
JOIN LoanPercentSettings s on s.Id = r.ProductSettingId
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                isActive,
                baseSettingId,
                accrualType,
                amountType,
                contractClass,
			}, UnitOfWork.Transaction).AsList();
        }

		public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            List<string> conditions = new List<string> { };

            bool? isActive = query?.Val<bool?>("IsActive");
            int? baseSettingId = query?.Val<int?>("BaseSettingId");
            AccrualType? accrualType = query?.Val<AccrualType?>("AccrualType");
            AmountType? amountType = query?.Val<AmountType?>("AmountType");

            if (baseSettingId.HasValue)
                conditions.Add("ab.BaseSettingId = @baseSettingId");
            if (isActive.HasValue)
                conditions.Add("ab.IsActive = @isActive");
            if (accrualType.HasValue)
                conditions.Add("ab.AccrualType = @accrualType");
            if (amountType.HasValue)
                conditions.Add("ab.AmountType = @amountType");

            string condition = string.Empty;

            if (conditions.Count > 0)
            {
                condition = "WHERE " + string.Join(" AND ", conditions);
            }

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM AccrualBases ab
{condition}", new
            {
                listQuery.Filter,
                isActive,
                baseSettingId,
                accrualType,
                amountType
            }, UnitOfWork.Transaction);
        }

    }
}