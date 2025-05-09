using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.Address;

namespace Pawnshop.Data.Access
{
    public class AddressRepository : RepositoryBase, IRepository<Address>
    {
        public AddressRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Address entity)
        {
            throw new System.NotImplementedException();
        }

        public void Update(Address entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public Address Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public List<Address> Find(object query)
        {
            var parentId = query?.Val<int?>("ParentId");

            return UnitOfWork.Session.Query<Address, AddressATEType, AddressGeonimType, Address>(@"
SELECT ad.*, atetype.*, geotype.*  FROM
(
SELECT a.Id, a.ParentId, a.ATETypeId, NULL AS GeonimTypeId, a.FullPathRus, FullPathKaz, NameRus, NameKaz, KATOCode, a.HasChild
FROM AddressATEs a
WHERE ParentId=1 AND @parentId IS NULL AND IsActual = 1
UNION ALL
SELECT  Id, ParentId, ATETypeId, NULL AS GeonimTypeId, FullPathRus, FullPathKaz, NameRus, NameKaz, KATOCode, a.HasChild
FROM AddressATEs a
WHERE ParentId=@parentId AND @parentId IS NOT NULL AND IsActual = 1
UNION ALL
SELECT Id, ParentId, ATETypeId, GeonimTypeId, FullPathRus, FullPathKaz, NameRus, NameKaz, KATOCode, a.HasChild
FROM AddressGeonims a
WHERE ATEId = @parentId AND IsActual = 1 AND ParentId IS NULL
UNION ALL
SELECT Id, ParentId, ATETypeId, GeonimTypeId, FullPathRus, FullPathKaz, NameRus, NameKaz, KATOCode, a.HasChild
FROM AddressGeonims a
WHERE ParentId = @parentId AND IsActual = 1
UNION ALL 
SELECT a.Id, ParentId, ATETypeId, GeonimTypeId, FullPathRus, FullPathKaz, a.NameRus, a.NameKaz, KATOCode, 0
FROM AddressGeonims a JOIN AddressGeonimTypes att ON a.GeonimTypeId = att.Id
WHERE ATEId = @parentId AND a.IsActual = 1 AND ParentId IS NULL AND a.HasChild = 1
--AND Att.Code in (11, 24)
UNION ALL
SELECT a.Id, ParentId, ATETypeId, GeonimTypeId, FullPathRus, FullPathKaz, a.NameRus, a.NameKaz, KATOCode, 0
FROM AddressGeonims a JOIN AddressGeonimTypes att ON a.GeonimTypeId = att.Id
WHERE ParentId = @parentId AND a.IsActual = 1 AND a.HasChild = 1
--AND Att.Code in (11, 24)
) AS ad
LEFT JOIN AddressATETypes atetype ON atetype.Id = ad.ATETypeId
LEFT JOIN AddressGeonimTypes geotype ON geotype.Id = ad.GeonimTypeId
ORDER BY ATETypeId DESC, ad.NameRus, ad.NameKaz", (a, at, gt) =>
            {
                a.ATEType = at;
                a.GeonimType = gt;
                return a;
            },new { parentId }).ToList();
        }

        public void CalculateHasChild()
        {
            UnitOfWork.Session.Execute(@"UPDATE a SET HasChild =
IIF((ISNULL((SELECT COUNT(*) FROM AddressATEs g WHERE g.ParentId=a.Id AND g.IsActual = 1),0) + ISNULL((SELECT COUNT(*) FROM AddressGeonims g WHERE g.ATEId=a.Id AND g.IsActual = 1),0))>0,1,0)
FROM AddressATEs a
UPDATE a SET HasChild =
IIF((ISNULL((SELECT COUNT(*) FROM AddressGeonims g WHERE g.ParentId=a.Id AND g.IsActual = 1),0))>0,1,0)
FROM AddressGeonims a", UnitOfWork.Transaction);
        }

        // У некоторых адресов не заполнены FullPathRus, FullPathKaz
        // заполняется в формате ', [NameRus типа АТЕ или типа геонима] [NameRus АТЕ или геонима]' для русского языка
        // ', [NameKaz АТЕ или геонима] [NameKaz типа АТЕ или типа геонима]' для казахского языка
        public void UpdateNullFullPath()
        {
            UnitOfWork.Session.Execute(@"UPDATE AddressGeonims SET FullPathKaz = N', ' + ag.NameKaz + N' ' + LOWER(agt.NameKaz)
FROM AddressGeonims ag JOIN AddressGeonimTypes agt ON ag.GeonimTypeId = agt.id 
WHERE ag.FullPathKaz IS NULL;
UPDATE AddressGeonims SET FullPathRus = N', ' + LOWER(agt.NameRus) + N' ' + ag.NameRus
  FROM AddressGeonims ag JOIN AddressGeonimTypes agt ON ag.GeonimTypeId = agt.id 
WHERE ag.FullPathRus IS NULL;
UPDATE AddressATEs SET FullPathKaz = N', ' + ag.NameKaz + N' ' + LOWER(agt.NameKaz)
  FROM AddressATEs ag JOIN AddressATETypes agt ON ag.ATETypeId = agt.id 
WHERE ag.FullPathKaz IS NULL;
UPDATE AddressATEs SET FullPathRus = N', ' + LOWER(agt.NameRus) + N' ' + ag.NameRus
  FROM AddressATEs ag JOIN AddressATETypes agt ON ag.ATETypeId = agt.id 
WHERE ag.FullPathRus IS NULL;", UnitOfWork.Transaction);
        }

        public List<Address> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        Address IRepository<Address>.Find(object query)
        {
            throw new NotImplementedException();
        }
    }
}