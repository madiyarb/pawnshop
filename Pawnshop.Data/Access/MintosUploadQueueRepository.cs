using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Mintos.UploadModels;

namespace Pawnshop.Data.Access
{
    public class MintosUploadQueueRepository : RepositoryBase, IRepository<MintosUploadQueue>
    {
        public MintosUploadQueueRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(MintosUploadQueue entity)
        {
            throw new System.NotImplementedException();
        }

        public void Update(MintosUploadQueue entity)
        {
            UnitOfWork.Session.Execute(@"
UPDATE MintosUploadQueue SET Status = @Status, UploadDate = @UploadDate, DeleteDate = @DeleteDate, MintosContractId = @MintosContractId
 WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public MintosUploadQueue Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public MintosUploadQueue Find(object query)
        {
            var contractId = query?.Val<int?>("ContractId");
            var uploadDate = query?.Val<DateTime?>("UploadDate");

            return UnitOfWork.Session.Query<MintosUploadQueue, Currency, MintosUploadQueue>(@"
SELECT *
FROM MintosUploadQueue muq
JOIN Currencies c ON c.Id = muq.CurrencyId
WHERE muq.DeleteDate IS NULL AND UploadDate IS NULL AND ContractId = @contractId AND CAST(CreateDate AS DATE) = CAST(@uploadDate AS DATE)", (muq, q) =>
            {
                muq.Currency = q;
                return muq;
            }, new { contractId, uploadDate }).FirstOrDefault();
        }

        public List<MintosUploadQueue> List(ListQuery listQuery, object query = null)
        {
            var status = query?.Val<MintosUploadStatus?>("Status");

            return UnitOfWork.Session.Query<MintosUploadQueue, Currency, MintosUploadQueue>(@"
SELECT *
FROM MintosUploadQueue muq
JOIN Currencies c ON c.Id = muq.CurrencyId
WHERE muq.DeleteDate IS NULL AND (@status IS NULL OR muq.Status = @status)", (muq, q) =>
            {
                muq.Currency = q;
                return muq;
            }, new { status }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public List<MintosUploadQueue> List(object query = null)
        {
            var status = query?.Val<MintosUploadStatus?>("Status");

            return UnitOfWork.Session.Query<MintosUploadQueue, Currency, MintosUploadQueue>(@"
SELECT *
FROM MintosUploadQueue muq
JOIN Currencies c ON c.Id = muq.CurrencyId
WHERE muq.DeleteDate IS NULL AND muq.Status = @status", (muq, q) =>
            {
                muq.Currency = q;
                return muq;
            }, new { status }).ToList();
        }
    }
}