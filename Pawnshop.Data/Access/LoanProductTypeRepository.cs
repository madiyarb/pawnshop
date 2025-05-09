using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class LoanProductTypeRepository : RepositoryBase, IRepository<LoanProductType>
    {
        public LoanProductTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(LoanProductType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO LoanProductTypes ( Name, NameAlt, Code, CollateralType, AuthorId, CreateDate ) VALUES ( @Name, @NameAlt, @Code, @CollateralType, @AuthorId, @CreateDate )
SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                if(entity.Accounts != null)
                    entity.Accounts.ForEach(account =>
                    {
                        account.ProductTypeId = entity.Id;
                        account.Id = InsertAccount(account);
                    });

                transaction.Commit();
            }
        }

        private int InsertAccount(LoanProductTypeAccount account)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO LoanProductTypeAccounts ( ProductTypeId, Name, Code, AccountId ) VALUES ( @ProductTypeId, @Name, @Code, @AccountId )
SELECT SCOPE_IDENTITY()", account, UnitOfWork.Transaction);
        }

        public void Update(LoanProductType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE LoanProductTypes SET Name = @Name, NameAlt = @NameAlt, Code = @Code, CollateralType = @CollateralType WHERE Id = @id", entity, UnitOfWork.Transaction);

                var toDelete = Get(entity.Id).Accounts.Where(w => !entity.Accounts.Any(x => w.Id == x.Id));
                foreach (var item  in toDelete)
                {
                    UnitOfWork.Session.Execute("DELETE FROM LoanProductTypeAccounts WHERE Id = @id", new { id = item.Id }, UnitOfWork.Transaction);
                }

                if (entity.Accounts != null)
                    entity.Accounts.ForEach(account =>
                    {
                        if (account.Id == 0)
                        {
                            account.ProductTypeId = entity.Id;
                            account.Id = InsertAccount(account);
                        }
                        else
                        {
                            UnitOfWork.Session.Execute("UPDATE LoanProductTypeAccounts SET Name = @Name, Code = @Code, AccountId = @AccountId WHERE Id = @id", account, UnitOfWork.Transaction);
                        }
                    });

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE LoanProductTypes SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public LoanProductType Get(int id)
        {
            return UnitOfWork.Session.Query<LoanProductType, User, LoanProductType>(@"
SELECT ls.*, u.*
  FROM LoanProductTypes ls
LEFT JOIN Users u ON u.Id = ls.AuthorId
WHERE ls.Id = @id", (h, u) =>
            {
                h.Author = u;
                h.Accounts = GetAccounts(id);
                return h;
            },new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        private List<LoanProductTypeAccount> GetAccounts(int typeId)
        {
            return UnitOfWork.Session.Query<LoanProductTypeAccount, Account, LoanProductTypeAccount>(@"
SELECT t.*, a.*
FROM LoanProductTypeAccounts t
LEFT JOIN Accounts a ON t.AccountId = a.Id
WHERE t.ProductTypeId=@typeId", (t, a) =>
                    {
                        t.Account = a;
                        return t;
                    }, new {typeId},
                    UnitOfWork.Transaction).ToList();
        }

        public LoanProductType Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<LoanProductType> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "ls.DeleteDate IS NULL";

            var condition = listQuery.Like(pre);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<LoanProductType, User, LoanProductType>($@"
SELECT ls.*, u.*
  FROM LoanProductTypes ls
LEFT JOIN Users u ON u.Id = ls.AuthorId
{condition} {order} {page}", (h, u) =>
            {
                h.Author = u;
                h.Accounts = GetAccounts(h.Id);
                return h;
            },new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "ls.DeleteDate IS NULL";

            var condition = listQuery.Like(pre);

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM LoanProductTypes ls
{condition}", new
            {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(q)
FROM (
SELECT COUNT(*) AS q
FROM Contracts
WHERE ProductTypeId = @id
UNION
SELECT COUNT(*) AS q
FROM LoanPercentSettings
WHERE ProductTypeId = @id) as t", new { id });
        }

        public List<LoanProductType> GetProductsByContractClass(ContractClass contractClass)
        {
            return UnitOfWork.Session.Query<LoanProductType>(@"
                SELECT distinct lpt.* FROM LoanProductTypes lpt
                inner join LoanPercentSettings lps on lps.ProductTypeId = lpt.id
                where lps.ContractClass = @contractClass", new { contractClass }, UnitOfWork.Transaction).ToList();
        }
    }
}