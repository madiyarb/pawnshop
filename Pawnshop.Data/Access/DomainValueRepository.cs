using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class DomainValueRepository : RepositoryBase, IRepository<DomainValue>
    {
        public DomainValueRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(DomainValue entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO DomainValues ( Name, NameAlt, Code, AuthorId, DomainCode, IsActive, AdditionalData, CreateDate)
                    VALUES ( @Name, @NameAlt, @Code, @AuthorId, @DomainCode, @IsActive, @AdditionalData, @CreateDate)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(DomainValue entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE DomainValues
                    SET 
                        Name = @Name, 
                        NameAlt = @NameAlt, 
                        Code = @Code,
                        AuthorId = @AuthorId, 
                        DomainCode = @DomainCode,
                        DeleteDate = @DeleteDate,
                        IsActive = @IsActive,
                        AdditionalData = @AdditionalData
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            DomainValue entity = Get(id);
            if (entity == null)
                throw new PawnshopApplicationException("Запись DomainValue не найдена");

            entity.DeleteDate = DateTime.Now;
            Update(entity);
        }

        public DomainValue Get(int id)
        {
            return UnitOfWork.Session.Query<DomainValue>(@"
                SELECT dv.*
                FROM DomainValues dv
                WHERE dv.Id=@id AND dv.DeleteDate IS NULL",
            new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public DomainValue Find(object query)
        {
            throw new NotImplementedException();
        }

        public DomainValue GetByCodeAndDomainCode(string code, string domainCode)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if(code.Trim() == string.Empty)
                throw new ArgumentException("Параметр не должен быть пустым", nameof(code));
            if (domainCode == null)
                throw new ArgumentNullException(nameof(domainCode));
            if (domainCode.Trim() == string.Empty)
                throw new ArgumentException("Параметр не должен быть пустым", nameof(domainCode));

            return UnitOfWork.Session.Query<DomainValue>(@"
                SELECT dv.*
                FROM DomainValues dv
                WHERE dv.Code=@code and dv.DomainCode=@domainCode
                AND dv.DeleteDate IS NULL",
                new { code, domainCode }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<DomainValue> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "dv.Name", "dv.NameAlt");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "dv.Id",
                Direction = SortDirection.Asc
            });

            bool? includeDeleted = query?.Val<bool?>("IncludeDeleted");
            if (!includeDeleted.HasValue || !includeDeleted.Value)
                condition += !string.IsNullOrEmpty(condition) ? " AND dv.DeleteDate IS NULL" : "dv.DeleteDate IS NULL";

            string domainCode = query?.Val<string>("DomainCode");
            if (!string.IsNullOrWhiteSpace(domainCode))
                condition += !string.IsNullOrEmpty(condition) ? " AND dv.DomainCode = @domainCode" : "dv.DomainCode = @DomainCode";

            if (condition != null && (condition.Length < 5 || condition.Substring(0, 5) != "WHERE"))
                condition = "WHERE " + condition;

            string pageQuery = listQuery.Page();
            return UnitOfWork.Session.Query<DomainValue>(@$"
                SELECT dv.*
                FROM DomainValues dv
                {condition} {order} {pageQuery}",
                new
                {
                    listQuery.Filter,
                    domainCode,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var condition = listQuery.Like(string.Empty, "dv.Name", "dv.NameAlt");
            bool? includeDeleted = query?.Val<bool?>("IncludeDeleted");
            if (!includeDeleted.HasValue || !includeDeleted.Value)
                condition += !string.IsNullOrEmpty(condition) ? " AND dv.DeleteDate IS NULL" : "dv.DeleteDate IS NULL";

            string domainCode = query?.Val<string>("DomainCode");
            if (!string.IsNullOrWhiteSpace(domainCode))
                condition += !string.IsNullOrEmpty(condition) ? " AND dv.DomainCode = @domainCode" : "dv.domainCode = @domainCode";

            if (condition != null && (condition.Length < 5 || condition.Substring(0, 5) != "WHERE"))
                condition = "WHERE " + condition;

            return UnitOfWork.Session.ExecuteScalar<int>(@$"
                SELECT COUNT(dv.Id)
                FROM DomainValues dv
                {condition}",
                new
                {
                    listQuery.Filter,
                    domainCode
                }, UnitOfWork.Transaction);
        }

        public List<DomainValue> ListOnlyManualBuyoutReasons()
        {
            List<string> automaticBuyoutReasons = new List<string>{ Constants.BUYOUT_ADDITION, Constants.BUYOUT_AUTOMATIC_BUYOUT, Constants.BUYOUT_PARTIAL_PAYMENT};
            var condition = " WHERE dv.DomainCode = 'BUYOUT_REASON' ";
            foreach (string reason in automaticBuyoutReasons)
            {
                condition += " AND dv.Code != '" + reason + "'";
            }

            return UnitOfWork.Session.Query<DomainValue>(@$"
                SELECT dv.*
                FROM DomainValues dv
                {condition}", UnitOfWork.Transaction).ToList();
        }
    }
}
