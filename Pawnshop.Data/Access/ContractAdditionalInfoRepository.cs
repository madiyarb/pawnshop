using Dapper;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ContractAdditionalInfoRepository : RepositoryBase, IRepository<ContractAdditionalInfo>
    {
        public ContractAdditionalInfoRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public ContractAdditionalInfo Find(object query)
        {
            var contractId = query.Val<int?>("ContractId");

            if (!contractId.HasValue)
                throw new PawnshopApplicationException("ContractId является обязательным полем!");

            return UnitOfWork.Session.QueryFirstOrDefault<ContractAdditionalInfo>(
"SELECT * FROM dogs.ContractAdditionalInfo WHERE Id = @contractId",
                new { contractId }, UnitOfWork.Transaction);
        }

        public ContractAdditionalInfo Get(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ContractAdditionalInfo>(
"SELECT * FROM dogs.ContractAdditionalInfo WHERE Id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public async Task<ContractAdditionalInfo> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ContractAdditionalInfo>(
"SELECT * FROM dogs.ContractAdditionalInfo WHERE Id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ContractAdditionalInfo entity)
        {
            UnitOfWork.Session.Query(@"INSERT INTO dogs.ContractAdditionalInfo ( Id, SmsCode, CbContractCode, SelectedBranchId, PartnerCode, ChangedControlDate, DateOfChangeControlDate, StorageListId, LoanStorageFileId )
VALUES ( @Id, @SmsCode, @CbContractCode, @SelectedBranchId, @PartnerCode, @ChangedControlDate, @DateOfChangeControlDate, @StorageListId, @LoanStorageFileId )",
                entity, UnitOfWork.Transaction);
        }

        public List<ContractAdditionalInfo> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public void Update(ContractAdditionalInfo entity)
        {
            UnitOfWork.Session.Query(@"UPDATE dogs.ContractAdditionalInfo
   SET SmsCode = @SmsCode,
       CbContractCode = @CbContractCode,
       SelectedBranchId = @SelectedBranchId,
       ClosedIsSend = @ClosedIsSend,
       PartnerCode = @PartnerCode,
       ChangedControlDate = @ChangedControlDate,
       DateOfChangeControlDate = @DateOfChangeControlDate,
       StorageListId = @StorageListId,
       LoanStorageFileId = @LoanStorageFileId
 WHERE Id = @Id",
                entity, UnitOfWork.Transaction);
        }
    }
}
