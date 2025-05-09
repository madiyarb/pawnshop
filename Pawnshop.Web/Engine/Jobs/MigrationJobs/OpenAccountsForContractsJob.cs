using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.MessageSenders;

namespace Pawnshop.Web.Engine.Jobs.MigrationJobs
{
    public class OpenAccountsForContractsJob
    {
        private readonly ContractRepository _contractRepository;
        private readonly GroupRepository _groupRepository;
        private readonly AccountService _accountService;
        private readonly EmailSender _emailSender;
        private readonly EventLog _eventLog;
        private readonly MigrationRepository _migrationRepository;

        public OpenAccountsForContractsJob(ContractRepository contractRepository, GroupRepository groupRepository,
            AccountService accountService, EmailSender emailSender, EventLog eventLog,
            MigrationRepository migrationRepository)
        {
            _contractRepository = contractRepository;
            _groupRepository = groupRepository;
            _accountService = accountService;
            _emailSender = emailSender;
            _eventLog = eventLog;
            _migrationRepository = migrationRepository;
        }

        public void Enqueue()
        {
            var branches = _groupRepository.List(new ListQuery {Page = null});
            foreach (var branch in branches.OrderBy(x=>x.Id))
            {
                BackgroundJob.Enqueue<OpenAccountsForContractsJob>(x => x.EnqueuePerBranch(branch.Id));
            }
        }

        public void EnqueuePerBranch(int branchId)
        {
            var contracts = new List<int>();
            //действующие на текущий момент
            var con1 = _migrationRepository.FindContractsWithoutAccounts(new
            {
                Status = ContractStatus.Signed,
                BranchId = branchId
            });
            contracts.AddRange(con1);

            //заключенные после 31.12.2020
            var con2 = _migrationRepository.FindContractsWithoutAccounts(new
            {
                BeginDate = new DateTime(2020, 12, 31),
                EndDate = DateTime.Now.AddDays(365),
                BranchId = branchId
            });
            contracts.AddRange(con2);

            //есть действия после после 01.10.2020
            var con3 = _migrationRepository.FindContractsWithoutAccounts(new
            {
                HasActionsAfterDate = new DateTime(2020, 12, 31),
                BranchId = branchId
            });
            contracts.AddRange(con3);
            
            // Отправленные на реализацию
            var con4 = _migrationRepository.FindContractsWithoutAccounts(new
            {
                Status = ContractStatus.SoldOut,
                BranchId = branchId
            });
            contracts.AddRange(con4);
            
            foreach (var contractId in contracts.Distinct())
            {
                BackgroundJob.Enqueue<OpenAccountsForContractsJob>(x => x.OpenForContract(contractId));
            }

        }

        public void OpenForContract(int contractId)
        {
            var isSuccess = true;
            try
            {
                var contract = _contractRepository.Get(contractId);
                _accountService.OpenForContract(contract);
                _eventLog.Log(EventCode.ContractOpenAccounts, EventStatus.Success, EntityType.Contract, contractId);

            }
            catch (Exception e)
            {
                isSuccess = false;
                SendWarningMessage(e.Message, e.StackTrace, contractId);
                _eventLog.Log(EventCode.ContractOpenAccounts, EventStatus.Failed, EntityType.Contract, contractId, e.Message, e.StackTrace);
            }

            if (isSuccess) BackgroundJob.Enqueue<MigrateActionsJob>(x => x.Migrate(contractId));
        }

        private void SendWarningMessage(string info, string message, int contractId)
        {
            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = "n.nikolay@tascredit.kz",
                ReceiverName = "Nick Nikolskiy",
                CopyAddresses = new List<MessageReceiver>
                {
                    new MessageReceiver
                    {
                        ReceiverAddress = "e.romanova@tascredit.kz",
                        ReceiverName = "Элона Романова"
                    }
                }
            };

            _emailSender.SendEmail($"Открытие счета для ({nameof(contractId)}={contractId}), ошибка: \"{info}\"", message, messageReceiver);
        }
    }
}
