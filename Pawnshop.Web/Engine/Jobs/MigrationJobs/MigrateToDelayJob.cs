using Pawnshop.Services.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Account.Internal;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Core;

namespace Pawnshop.Web.Engine.Jobs.MigrationJobs
{
    public class MigrateToDelayJob
    {
        private readonly IContractService _contractService;
        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
        private readonly MigrationRepository _migrationRepository;

        public MigrateToDelayJob(IContractService contractService, IDictionaryWithSearchService<Group, BranchFilter> branchService,
            MigrationRepository migrationRepository)
        {
            _contractService = contractService;
            _branchService = branchService;
            _migrationRepository = migrationRepository;
        }

        public void Execute()
        {
            var branches = _branchService.List(new ListQuery { Page = null });

            foreach (var branch in branches.List)
            {
                BackgroundJob.Enqueue<MigrateToDelayJob>(x => x.EnqueuePerBranch(branch.Id));
            }
        }

        public void EnqueuePerBranch(int branchId)
        {
            var statuses = new List<ContractStatus> {ContractStatus.Signed, ContractStatus.SoldOut};//статусы - действующий и отправлен на реализацию

            List<DateTime> dates = _migrationRepository.FindLateNextPaymentDates(branchId, DateTime.Now, statuses);

            foreach (var date in dates)
            {
                var filter = new ContractFilter
                {
                    NextPaymentDate = date,//дата оплаты была вчера
                    Statuses = statuses,
                    OwnerIds = new[] { branchId }//текущий филиал
                };

                if (_contractService.Count(filter) > 0)
                {
                    var contracts = _contractService.List(filter);

                    foreach (var contract in contracts)
                    {
                        BackgroundJob.Enqueue<MigrateToDelayJob>(x => x.EnqueuePerContract(contract.Id));
                    }
                }
            }
        }

        public void EnqueuePerContract(int contractId)
        {
            Contract contract = _contractService.Get(contractId);

            BackgroundJob.Enqueue<ITakeAwayToDelay>(x => x.TakeAwayToDelay(contract, contract.NextPaymentDate.Value.AddDays(1), contract.NextPaymentDate.Value.AddDays(1), Constants.ADMINISTRATOR_IDENTITY ,true));

        }
    }
}
