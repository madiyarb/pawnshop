using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class CurrencyRepository : RepositoryBase, IRepository<Currency>
    {
        public CurrencyRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Currency entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Currencies ( Name, Code, IsDefault, CurrentNationalBankExchangeRate, CurrentNationalBankExchangeQuantity, LastUpdate)
VALUES ( @Name, @Code, @IsDefault, @CurrentNationalBankExchangeRate, @CurrentNationalBankExchangeQuantity, dbo.GETASTANADATE())
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Currency entity)
        {
                UnitOfWork.Session.Execute(@"
INSERT INTO CurrenciesLog ( CurrencyId, ActionType, Name, Code, IsDefault, NationalBankExchangeRate, NationalBankExchangeQuantity, LastUpdate, Date)
SELECT Id, N'UPDATE' ActionType, Name, Code, IsDefault, CurrentNationalBankExchangeRate, CurrentNationalBankExchangeQuantity, LastUpdate, dbo.GETASTANADATE()
FROM Currencies
WHERE Id = @Id

UPDATE Currencies
SET Name = @Name, Code = @Code, IsDefault = @IsDefault, CurrentNationalBankExchangeRate = @CurrentNationalBankExchangeRate,
CurrentNationalBankExchangeQuantity = @CurrentNationalBankExchangeQuantity, LastUpdate = dbo.GETASTANADATE()
WHERE Id = @Id", entity, UnitOfWork.Transaction);

        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Currency Get(int id)
        {
            return UnitOfWork.Session.Query<Currency>($@"
SELECT *
  FROM Currencies c WHERE Id = @id", new
            {
                id
            }, UnitOfWork.Transaction).FirstOrDefault();
        }


        public Currency GetFromHistory(int id, DateTime date)
        {
            return UnitOfWork.Session.Query<Currency>($@"
SELECT TOP 1 CurrencyId AS Id, Name, Code, IsDefault, NationalBankExchangeRate AS CurrentNationalBankExchangeRate, NationalBankExchangeQuantity AS CurrentNationalBankExchangeQuantity, LastUpdate
FROM CurrenciesLog c
WHERE CurrencyId = @id AND @date BETWEEN LastUpdate AND Date", new
            {
                id,
                date
            }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public Currency Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var isDefault = query.Val<bool>("IsDefault");

            var condition = "WHERE c.IsDefault = @isDefault";

            return UnitOfWork.Session.Query<Currency>($@"
SELECT *
  FROM Currencies c
{condition}", new
            {
                isDefault
            }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<Currency> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var isDefault = query?.Val<bool?>("IsDefault");

            var pre = "c.Id>0";
            pre += isDefault.HasValue ? " AND IsPersonal = @isDefault" : string.Empty;

            var condition = listQuery.Like(pre, "c.Code", "c.Name");

            return UnitOfWork.Session.Query<Currency>($@"
SELECT *
  FROM Currencies c
{condition}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                isDefault
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var isDefault = query?.Val<bool?>("IsDefault");

            var pre = "c.Id>0";
            pre += isDefault.HasValue ? " AND IsPersonal = @isDefault" : string.Empty;

            var condition = listQuery.Like(pre, "c.Code", "c.Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM Currencies c
{condition}", new
            {
                listQuery.Filter,
                isDefault
            }, UnitOfWork.Transaction);
        }
    }
}