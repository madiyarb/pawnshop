using System;
using Pawnshop.Data.Access;
using Pawnshop.Core.Queries;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Hangfire;
using Pawnshop.Data.Models.CreditBureaus;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CBBatchDataFulfillJob
    {
        private readonly CBBatchRepository _cbBatchRepository;
        private readonly CBContractRepository _cbContractRepository;
        private readonly EnviromentAccessOptions _options;
        private readonly JobLog _jobLog;

        public CBBatchDataFulfillJob(
            IOptions<EnviromentAccessOptions> options,
            CBBatchRepository cbBatchRepository,
            CBContractRepository cbContractRepository,
            JobLog jobLog)
        {
            _options = options.Value;
            _cbBatchRepository = cbBatchRepository;
            _cbContractRepository = cbContractRepository;
            _jobLog = jobLog;
        }

        [Queue("cb")]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Execute()
        {
            if (!_options.CBUpload) return;

            try
            {
                _jobLog.Log("CBBatchDataFulfillJob", JobCode.Begin, JobStatus.Success);

                var batches = _cbBatchRepository.List(new ListQuery() { Page = null }, new { Status = CBBatchStatus.Created });

                foreach (var batch in batches)
                {
                    try
                    {
                        _cbBatchRepository.Fulfill(batch);
                    }
                    catch (Exception e)
                    {
                        _jobLog.Log("CBBatchDataFulfillJob", JobCode.Error, JobStatus.Failed, responseData: e.Message);

                        using (var transaction = _cbContractRepository.BeginTransaction())
                        {
                            batch.BatchStatusId = CBBatchStatus.FulfillError;
                            _cbBatchRepository.Update(batch);

                            transaction.Commit();
                        }
                    }
                    
                }
                _jobLog.Log("CBBatchDataFulfillJob", JobCode.End, JobStatus.Success);
            }
            catch (Exception e)
            {
                _jobLog.Log("CBBatchDataFulfillJob", JobCode.Error, JobStatus.Failed, responseData: e.Message);
            }
            finally
            {
                BackgroundJob.Enqueue<CBXMLFileCreationJob>(x => x.Execute());
            }
        }
    }    
}
