using System;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Core;

namespace Pawnshop.Web.Engine.Jobs
{
    public class AutocreditContractCancelJob
    {
        private readonly AutocreditContractCancelRepository _autocreditContractCancelRepository;
        private readonly JobLog _jobLog;
        public AutocreditContractCancelJob(
            AutocreditContractCancelRepository autocreditContractCancelRepository,
            JobLog jobLog
            ) 
        {
            _autocreditContractCancelRepository = autocreditContractCancelRepository;
            _jobLog = jobLog;
        }
        public void Execute()
        {
            try
            {
                var contractsToCancel = _autocreditContractCancelRepository.Find();
                foreach ( var contractId in contractsToCancel )
                {
                    _jobLog.Log("AutocreditContractCancelJob", JobCode.Start, JobStatus.Success, EntityType.Contract, entityId: contractId);
                    _autocreditContractCancelRepository.CancelContract(contractId);
                    _jobLog.Log("AccountantIntegrationJob", JobCode.End, JobStatus.Success, EntityType.Contract, entityId: contractId);
                }
            }
            catch (Exception ex)
            {
                _jobLog.Log("AccountantIntegrationJob", JobCode.Error, JobStatus.Failed, EntityType.Contract, responseData: ex.Message);
            }
        }
    }
}
