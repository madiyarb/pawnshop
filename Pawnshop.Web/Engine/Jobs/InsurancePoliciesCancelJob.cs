using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using System;
using System.Linq;
using Hangfire;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Access;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Web.Engine.Jobs
{
    public class InsurancePoliciesCancelJob
    {
        private readonly EventLog _eventLog;
        private readonly JobLog _jobLog;
        private readonly InsurancePoliceRequestRepository _insurancePoliceRepository;
        private readonly IInsuranceService _insuranceService;

        public InsurancePoliciesCancelJob(EventLog eventLog,
                                          JobLog jobLog,
                                          InsurancePoliceRequestRepository insurancePoliceRepository,
                                          IInsuranceService insuranceService
                                          )
        {
            _eventLog = eventLog;
            _jobLog = jobLog;
            _insurancePoliceRepository = insurancePoliceRepository;
            _insuranceService = insuranceService;
        }

        [Queue("insurance")]
        public void Execute()
        {
            try
            {
                _jobLog.Log("InsurancePoliciesCancelJob", JobCode.Start, JobStatus.Success, requestData: "Запуск джоба для отмены страховых полисов");

                var requestsForCancel = _insurancePoliceRepository.GetInsurancePolicyRequestsToCancel();

                if (!requestsForCancel.Any())
                {
                    _jobLog.Log("InsurancePoliciesCancelJob", JobCode.End, JobStatus.Success, responseData: "Список страховых полисов для отмены пуст");
                    return;
                }

                foreach (var request in requestsForCancel)
                {
                    try
                    {
                        _insuranceService.BPMCancelPolicy(request);

                        _eventLog.Log(
                           EventCode.CancelInsurancePolice,
                           EventStatus.Success,
                           EntityType.InsurancePolicy,
                           entityId: request.ContractId,
                           responseData: JsonConvert.SerializeObject(request),
                           uri: GetUrl(request.ContractId),
                           userId: Constants.ADMINISTRATOR_IDENTITY
                        );
                    }
                    catch (Exception e)
                    {
                        _eventLog.Log(
                            EventCode.CancelInsurancePolice,
                            EventStatus.Failed,
                            EntityType.InsurancePolicy,
                            entityId: request.ContractId,
                            responseData: JsonConvert.SerializeObject(e),
                            uri: GetUrl(request.ContractId),
                            userId: Constants.ADMINISTRATOR_IDENTITY
                        );
                    }
                }

                _jobLog.Log("InsurancePoliciesCancelJob", JobCode.End, JobStatus.Success, requestData: @$"На дату {DateTime.Now.ToString("dd.MM.yyy")} отменено {requestsForCancel.Count()} полисов");

            }
            catch (Exception e)
            {
                _jobLog.Log("InsurancePoliciesCancelJob", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(e));
            }
        }

        private string GetUrl(int contractId)
        {
            return $"http://fc.tas.kz/contracts/{contractId}";
        }
    }
}
