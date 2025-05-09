using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Migrate;

namespace Pawnshop.Web.Engine.Jobs.MigrationJobs
{
    public class RecalculateAccountBalanceJob
    {
        private readonly MigrationRepository _migrationRepository;
        private readonly IDictionaryService<AccountPlan> _accountPlanService;

        public RecalculateAccountBalanceJob(MigrationRepository migrationRepository, IDictionaryService<AccountPlan> accountPlanService)
        {
            _migrationRepository = migrationRepository;
            _accountPlanService = accountPlanService;
        }

        public void EnqueueAll()
        {
            var accountPlans = _accountPlanService.List(new ListQuery {Page = null}).List;

            //Счета без привязанного плана счетов
            BackgroundJob.Enqueue<RecalculateAccountBalanceJob>(x => x.EnqueuePerAccountPlan(null));

            //Счета с привязанным планом счетов
            foreach (var plan in accountPlans.OrderBy(x=>x.Code))
            {
                BackgroundJob.Enqueue<RecalculateAccountBalanceJob>(x => x.EnqueuePerAccountPlan(plan.Id));
            }
        }

        public void EnqueuePerAccountPlan(int? accountPlanId)
        {
            //Ищем все счета по плану
            var queue = _migrationRepository.FindAccountsWithRecords(accountPlanId);

            foreach (var accountId in queue)
            {
                BackgroundJob.Enqueue<IAccountRecordService>(x => x.RecalculateBalanceOnAccount(accountId, false, null));
            }
        }
    }
}
