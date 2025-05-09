using Pawnshop.Core;
using System;
using System.Collections.Generic;
using Pawnshop.Core.Impl;
using Dapper;
using Pawnshop.Data.Models.CreditBureaus;

namespace Pawnshop.Data.Access
{
    public class CBBatchContractsUploadRepository : RepositoryBase
    {
        public CBBatchContractsUploadRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<CBContractUploadModel> List()
        {
            return UnitOfWork.Session.Query<CBContractUploadModel>(@"
                SELECT 
                    el.Id as EventLogId,
                    c.Id as ContractId, 
                    c.ContractClass as ContractClass, 
                    cbc.CBBatchId as BatchId, 
                    el.ResponseData as ResponseData, 
                    g.DisplayName as BranchName, 
                    cl.Id as ClientId, 
                    cl.IdentityNumber as IIN,
	                el.CreateDate as CreateDate
                FROM CBContracts cbc
                JOIN Contracts c ON c.Id = cbc.ContractId
                JOIN Clients cl ON cl.Id = c.ClientId
                JOIN EventLogItems el ON el.EntityId = cbc.Id
                JOIN Groups g ON g.Id = c.BranchId
                WHERE el.EventCode = 801 
                ORDER BY cbc.Id ASC
                ", UnitOfWork.Transaction).AsList();
        }

        public void SetUploadedStatus(List<int> list)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE EventLogItems SET EventCode = 807 WHERE Id in @list
                ", new { list = list }, UnitOfWork.Transaction);
        }
    }
}
