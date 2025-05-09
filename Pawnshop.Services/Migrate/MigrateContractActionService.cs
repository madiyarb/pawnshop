using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Migration;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Migrate
{
    public class MigrateContractActionService : IService
    {
        private readonly ICashOrderService _cashOrderService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> _businessOperationSettingService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly IAccountPlanSettingService _accountPlanSettingService;
        private readonly IAccountService _accountService;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly ContractActionRowRepository _contractActionRowRepository;
        private readonly MigrationRepository _migrationRepository;

        public MigrateContractActionService(ICashOrderService cashOrderService,
            IBusinessOperationService businessOperationService,
            IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> businessOperationSettingService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            IAccountPlanSettingService accountPlanSettingService,
            IAccountService accountService,
            ContractActionRepository contractActionRepository, MigrationRepository migrationRepository,
            ContractActionRowRepository contractActionRowRepository)
        {
            _cashOrderService = cashOrderService;
            _businessOperationService = businessOperationService;
            _businessOperationSettingService = businessOperationSettingService;
            _accountSettingService = accountSettingService;
            _accountPlanSettingService = accountPlanSettingService;
            _accountService = accountService;
            _contractActionRepository = contractActionRepository;
            _migrationRepository = migrationRepository;
            _contractActionRowRepository = contractActionRowRepository;
        }

        public void MigrateActions(Contract contract)
        {
            using (var transaction = _contractActionRepository.BeginTransaction())
            {
                foreach (var action in contract.Actions)
                {
                    Migrate(contract, action);
                }

                transaction.Commit();
            }
        }


        public void Migrate(Contract contract, ContractAction action)
        {
            var _accounts = _accountService.List(new ListQueryModel<AccountFilter>
            {
                Page = null,
                Model = new AccountFilter
                {
                    ContractId = contract.Id
                }
            }).List;

                foreach (var row in action.Rows)
                {
                    var order = _cashOrderService.GetAsync(row.OrderId).Result;
                    if (order.BusinessOperationId.HasValue || order.BusinessOperationSettingId.HasValue || order.ContractActionId.HasValue) continue;

                    if (order.DebitAccountId != row.DebitAccountId || order.CreditAccountId != row.CreditAccountId) throw new PawnshopApplicationException("Ошибка, счета в действии и в кассовом ордере не сходятся!");

                    var migration = _migrationRepository.TryToFind(new MigrationOperationSettingFilter
                    {
                        ActionType = action.ActionType,
                        PaymentType = row.PaymentType,
                        DebitAccountId = row.DebitAccountId,
                        CreditAccountId = row.CreditAccountId
                    });

                    if (!migration.BusinessOperationId.HasValue) throw new PawnshopApplicationException("В настройках миграции не проставлен идентификатор бизнес-операции BusinessOperationId");
                    var operation = _businessOperationService.GetAsync(migration.BusinessOperationId.Value).Result;

                    if (!migration.BusinessOperationSettingId.HasValue) throw new PawnshopApplicationException("В настройках миграции не проставлен идентификатор настройки бизнес-операции BusinessOperationSettingId");
                    var setting = _businessOperationSettingService.GetAsync(migration.BusinessOperationSettingId.Value).Result;

                    if (row.CreditAccountId.HasValue)
                    {
                        var creditSetting = _accountSettingService.GetAsync(setting.CreditSettingId.Value).Result;

                        //var previousCreditAccount = _accountService.GetAsync(row.CreditAccountId.Value).Result;
                        if (creditSetting.IsConsolidated)
                        {
                            row.CreditAccountId = _accountPlanSettingService.Find(contract.Branch.OrganizationId, creditSetting.Id,
                                contract.BranchId, contract.ContractTypeId, contract.PeriodTypeId).AccountId;
                        }
                        else
                        {
                            row.CreditAccountId = _accounts.FirstOrDefault(x => x.AccountSettingId == creditSetting.Id)?.Id;
                        }

                        if (row.CreditAccountId.HasValue)
                        {
                            var account = _accountService.GetAsync(row.CreditAccountId.Value).Result;
                            order.CreditAccountId = row.CreditAccountId;
                            row.CreditAccount = account;
                            order.CreditAccount = account;
                        }

                    }

                    if (row.DebitAccountId.HasValue)
                    {
                        var debitSetting = _accountSettingService.GetAsync(setting.DebitSettingId.Value).Result;

                        if (debitSetting.IsConsolidated)
                        {
                            row.DebitAccountId = _accountPlanSettingService.Find(contract.Branch.OrganizationId, debitSetting.Id,
                                contract.BranchId, contract.ContractTypeId, contract.PeriodTypeId).AccountId;
                        }
                        else
                        {
                            row.DebitAccountId = _accounts.FirstOrDefault(x => x.AccountSettingId == debitSetting.Id)?.Id;
                        }

                        if (row.DebitAccountId.HasValue)
                        {
                            var account = _accountService.GetAsync(row.DebitAccountId.Value).Result;
                            order.DebitAccountId = row.DebitAccountId;
                            row.DebitAccount = account;
                            order.DebitAccount = account;
                        }
                    }

                    using (var transaction = _cashOrderService.BeginCashOrderTransaction())
                    {
                        _contractActionRowRepository.Update(row);

                        order.ContractId = contract.Id;
                        order.BusinessOperationId = operation.Id;
                        order.BusinessOperationSettingId = setting.Id;
                        order.ContractActionId = action.Id;
                        order.CurrencyId = 1;//TODO:переделать
                        
                        _cashOrderService.Migrate(order);
                        transaction.Commit();
                    }
                }
        }

        private string FindBusinessOperationCodeForMigration(ContractAction action)
        {
            return action.ActionType switch
            {
                ContractActionType.Sign => "SIGN_MIGRATION",
                ContractActionType.Prolong => "PROLONG_MIGRATION",
                ContractActionType.Buyout => "BUYOUT_MIGRATION",
                ContractActionType.PartialBuyout => "PARTIALBUYOUT_MIGRATION",
                ContractActionType.PartialPayment => "PARTIALPAYMENT_MIGRATION",
                ContractActionType.Selling => "SELLING_MIGRATION",
                ContractActionType.Transfer => "TRANSFER_MIGRATION",
                ContractActionType.MonthlyPayment => "MONTHLYPAYMENT_MIGRATION",
                ContractActionType.Addition => "ADDITION_MIGRATION",
                ContractActionType.Prepayment => action.IsInitialFee.HasValue && action.IsInitialFee.Value ? "INITIALFEE_MIGRATION" : "PREPAYMENT_MIGRATION",
                ContractActionType.Refinance => "REFINANCE_MIGRATION",
                ContractActionType.PrepaymentReturn => "PREPAYMENTRETURN_MIGRATION",
                _ => throw new ArgumentOutOfRangeException(nameof(action.ActionType), action.ActionType, null)
            };
        }
    }
}
