using System;
using System.Collections.Generic;
using Pawnshop.Data.Access;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Core.Exceptions;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Hangfire;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CreditBureaus;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Web.Models.Contract;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CBBatchCreationDailyJob
    {
        private readonly CBBatchRepository _cbBatchRepository;
        private readonly CBContractRepository _cbContractRepository;
        private readonly ContractRepository _contractRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly EventLog _eventLog;
        private readonly JobLog _jobLog;
        private readonly EnviromentAccessOptions _options;
        private readonly DateTime _date = DateTime.Today.AddSeconds(-1);
        private readonly DateTime _dateFrom = new DateTime(2020, 6, 30);

        public CBBatchCreationDailyJob(IOptions<EnviromentAccessOptions> options, EventLog eventLog, OrganizationRepository organizationRepository,
            GroupRepository groupRepository, ContractRepository contractRepository, CBBatchRepository cbBatchRepository,
            CBContractRepository cbContractRepository, JobLog jobLog)
        {
            _contractRepository = contractRepository;
            _organizationRepository = organizationRepository;
            _eventLog = eventLog;
            _jobLog = jobLog;
            _options = options.Value;
            _cbBatchRepository = cbBatchRepository;
            _cbContractRepository = cbContractRepository;
        }

        public void Execute()
        {
            if (!_options.CBUpload)
            {
                return;
            }
            try
            {
                var organizations = _organizationRepository.List(new ListQuery() { Page = null });
                var cbs = new List<CBType> { CBType.FCB, CBType.SCB };

                foreach (var cb in cbs)
                {
                    _cbBatchRepository.FillCBBatchesDaily((int)cb);
                }

                _jobLog.Log("CBBatchCreationDailyJob", JobCode.End, JobStatus.Success);
            }
            catch (Exception e)
            {
                _jobLog.Log("CBBatchCreationDailyJob", JobCode.Error, JobStatus.Failed, requestData: e.Message);
            }
            finally
            {
                BackgroundJob.Enqueue<CBBatchDataFulfillJob>(x => x.Execute());
            }
        }
    }
}
