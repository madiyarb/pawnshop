using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class AttractionChannelRepository : RepositoryBase, IRepository<AttractionChannel>
    {
        public AttractionChannelRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AttractionChannel entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO AttractionChannels ( Name, NameAlt, NeedsMore, Disabled ) VALUES ( @Name, @NameAlt, @NeedsMore, @Disabled )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(AttractionChannel entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE AttractionChannels SET Name = @Name, NameAlt = @NameAlt, NeedsMore = @NeedsMore, Disabled = @Disabled WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE AttractionChannels SET DeleteDate=dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public AttractionChannel Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AttractionChannel>(@"
SELECT *
FROM AttractionChannels
WHERE Id = @id", new { id });
        }

        public int GetIdByCode(string code)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<int>(@"
SELECT id
FROM AttractionChannels
WHERE code LIKE @Code", new { Code = $"%{code}%" });
        }

        public AttractionChannel Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<AttractionChannel> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";
            var disabled = query?.Val<bool>("Disabled");
            if (disabled.HasValue) pre += " AND Disabled = @disabled";

            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<AttractionChannel>($@"
SELECT *
FROM AttractionChannels
{condition} {order} {page}", new
            {
                disabled,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";
            var disabled = query?.Val<bool>("Disabled");
            if (disabled.HasValue) pre += " AND Disabled = @disabled";

            var condition = listQuery.Like(pre, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM AttractionChannels
{condition}", new
            {
                listQuery.Filter
            });
        }
    }
}