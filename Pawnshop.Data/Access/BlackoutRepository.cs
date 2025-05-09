using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class BlackoutRepository : RepositoryBase, IRepository<Blackout>
    {
        public BlackoutRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Blackout entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Blackouts
( BeginDate, EndDate, Note, LoanPercent, PenaltyPercent, IsPersonal )
VALUES ( @BeginDate, @EndDate, @Note, @LoanPercent, @PenaltyPercent, @IsPersonal )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Blackout entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Blackouts
SET BeginDate = @BeginDate, EndDate = @EndDate, Note = @Note,
LoanPercent = @LoanPercent, PenaltyPercent = @PenaltyPercent,
IsPersonal = @IsPersonal
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Blackout Get(int id)
        {
            return UnitOfWork.Session.Query<Blackout>(@"
SELECT *
FROM Blackouts 
WHERE Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public Blackout Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Blackout> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var date = query?.Val<DateTime?>("Date");
            var isPersonal = query?.Val<bool?>("IsPersonal");

            var pre = "b.Id>0";
            pre += date.HasValue ? " AND @date BETWEEN b.BeginDate AND b.EndDate" : string.Empty;
            pre += isPersonal.HasValue ? " AND IsPersonal = @isPersonal" : string.Empty;

            var condition = listQuery.Like(pre, "b.Note");

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "BeginDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Blackout>($@"
SELECT *
FROM Blackouts b
{condition}
{order}",new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                date,
                isPersonal
            }, UnitOfWork.Transaction).ToList();  
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var date = query?.Val<DateTime?>("Date");
            var isPersonal = query?.Val<bool?>("IsPersonal");

            var pre = "b.Id>0";
            pre += date.HasValue ? " AND @date BETWEEN b.BeginDate AND b.EndDate" : string.Empty;
            pre += isPersonal.HasValue ? " AND IsPersonal = @isPersonal" : string.Empty;

            var condition = listQuery.Like(pre, "b.Note");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM Blackouts b
{condition}
",new 
            {
                listQuery.Filter,
                date,
                isPersonal
            }, UnitOfWork.Transaction);
        }
    }
}