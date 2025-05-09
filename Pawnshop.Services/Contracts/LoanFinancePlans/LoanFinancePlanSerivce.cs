using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.LoanFinancePlans;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Services.Domains;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Services.Contracts.LoanFinancePlans
{
    public class LoanFinancePlanSerivce : BaseService<LoanFinancePlan>, ILoanFinancePlanSerivce
    {
        private readonly ContractRepository _contractRepository;
        private readonly IContractService _contractService;
        private readonly ISessionContext _sessionContext;
        private readonly IDomainService _domainService;

        public LoanFinancePlanSerivce(IRepository<LoanFinancePlan> repository, ContractRepository contractRepository, ISessionContext sessionContext,
            IDomainService domainService, IContractService contractService) : base(repository)
        {
            _contractRepository = contractRepository;
            _contractService = contractService;
            _sessionContext = sessionContext;
            _domainService = domainService;
        }

        public List<LoanFinancePlan> GetList(int contractId)
        {
            if (contractId <= 0)
                throw new ArgumentNullException(nameof(contractId));

            _contractService.GetOnlyContract(contractId);

            List<LoanFinancePlan> loanFinancePlans = _repository.List(new ListQuery(), new { ContractId = contractId });

            return loanFinancePlans;
        }

        private void ValidateLoanFinancePlan(int contractId, List<LoanFinancePlan> loanFinancePlans)
        {
            if (loanFinancePlans == null)
                throw new ArgumentNullException(nameof(contractId));

            var contract = _contractService.GetOnlyContract(contractId);
            var errors = new HashSet<string>();

            decimal debtFundsSum = loanFinancePlans.Sum(fp => fp.DebtFunds);
            if (debtFundsSum > contract.LoanCost)
                errors.Add("Сумма всех заемных денег не должна превышать суммы займа по Договору");

            Dictionary<int, DomainValue> loanFinancePlanDomainValuesDict = _domainService.GetDomainValues(Constants.LOAN_PURPOSE_DOMAIN_VALUE).ToDictionary(dv => dv.Id, dv => dv);

            if (loanFinancePlans.Where(e => e.Id != 0).GroupBy(e => e.Id).Any(e => e.Count() > 1))
                errors.Add("Обнаружены несколько записей с одним Id, обратитесь в тех. поддержку");

            // проверим на null значения в поле LoanPurposeId
            bool nullLoanPurposeIdExists = loanFinancePlans.Any(e => e.LoanPurposeId == 0);
            if (nullLoanPurposeIdExists)
                errors.Add($"Не все ФП имеют заполненный {nameof(LoanFinancePlan.LoanPurposeId)}");

            // проверим на null значения в поле Description
            bool nullDescriptionExists = loanFinancePlans.Any(e => String.IsNullOrEmpty(e.Description));
            if (nullDescriptionExists)
                errors.Add($"Не все ФП имеют заполненный {nameof(LoanFinancePlan.Description)}");

            // проверим на null значения в поле DebtFunds
            bool nullDebtFundsExists = loanFinancePlans.Any(e => e.DebtFunds == 0);
            if (nullDebtFundsExists)
                errors.Add($"Не все ФП имеют заполненный {nameof(LoanFinancePlan.DebtFunds)}");

            // проверим чтобы значения LoanPurposeId были валидными
            HashSet<int> uniqueLoanFinancePlanIds = loanFinancePlans.Select(e => e.LoanPurposeId).ToHashSet();
            if (!uniqueLoanFinancePlanIds.IsSubsetOf(loanFinancePlanDomainValuesDict.Keys))
                errors.Add($"Не все ФП имеют правильный {nameof(LoanFinancePlan.LoanPurposeId)}");

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }

        private bool CheckLoanFinancePlanChangedFromDBModel(LoanFinancePlan loanFinancePlan, LoanFinancePlan loanFinancePlanFromDB)
        {
            if (loanFinancePlan == null)
                throw new ArgumentNullException(nameof(loanFinancePlan));

            if (loanFinancePlanFromDB == null)
                throw new ArgumentNullException(nameof(loanFinancePlanFromDB));

            if (loanFinancePlan.Id != loanFinancePlanFromDB.Id)
                throw new InvalidOperationException($"{nameof(loanFinancePlan)}.{nameof(loanFinancePlan.Id)} должен быть равен {nameof(loanFinancePlanFromDB)}.{nameof(loanFinancePlanFromDB.Id)}");

            return loanFinancePlanFromDB.LoanPurposeId != loanFinancePlan.LoanPurposeId ||
                   loanFinancePlanFromDB.OwnFunds != loanFinancePlan.OwnFunds ||
                   loanFinancePlanFromDB.DebtFunds != loanFinancePlan.DebtFunds ||
                   loanFinancePlanFromDB.Description != loanFinancePlan.Description;
        }

        public List<LoanFinancePlan> SaveFinancePlans(int contractId, List<LoanFinancePlan> loanFinancePlansRequest)
        {
            if (contractId <= 0)
                throw new ArgumentNullException(nameof(contractId));

            Dictionary<int, LoanFinancePlan> loanFinancePlansFromDB = _repository.List(new ListQuery(), new { ContractId = contractId }).ToDictionary(c => c.Id, c => c);
            ValidateLoanFinancePlan(contractId, loanFinancePlansRequest);

            var syncedLoanFinancePlans = new List<LoanFinancePlan>();

            foreach (var loanFinancePlan in loanFinancePlansRequest)
            {
                LoanFinancePlan loanFinancePlanFromDB = null;
                if (!loanFinancePlansFromDB.TryGetValue(loanFinancePlan.Id, out loanFinancePlanFromDB))
                {
                    loanFinancePlanFromDB = new LoanFinancePlan
                    {
                        ContractId = contractId,
                        LoanPurposeId = loanFinancePlan.LoanPurposeId,
                        Description = loanFinancePlan.Description,
                        OwnFunds = loanFinancePlan.OwnFunds,
                        DebtFunds = loanFinancePlan.DebtFunds,
                        CreateDate = DateTime.Now,
                        AuthorId = _sessionContext.UserId
                    };
                }
                else
                {
                    if (CheckLoanFinancePlanChangedFromDBModel(loanFinancePlan, loanFinancePlanFromDB))
                    {
                        loanFinancePlanFromDB.LoanPurposeId = loanFinancePlan.LoanPurposeId;
                        loanFinancePlanFromDB.LoanPurposeId = loanFinancePlan.LoanPurposeId;
                        loanFinancePlanFromDB.Description = loanFinancePlan.Description;
                        loanFinancePlanFromDB.OwnFunds = loanFinancePlan.OwnFunds;
                        loanFinancePlanFromDB.DebtFunds = loanFinancePlan.DebtFunds;
                    }

                    loanFinancePlansFromDB.Remove(loanFinancePlan.Id);
                }
                syncedLoanFinancePlans.Add(loanFinancePlanFromDB);
            }

            if (loanFinancePlansFromDB.Count > 0 || syncedLoanFinancePlans.Count > 0)
                using (var transaction = _repository.BeginTransaction())
                {
                    foreach ((int key, var loanFinancePlan) in loanFinancePlansFromDB)
                    {
                        _repository.Delete(key);
                    }

                    foreach (var loanFinancePlan in syncedLoanFinancePlans)
                    {
                        if (loanFinancePlan.Id != default)
                        {
                            _repository.Update(loanFinancePlan);
                        }
                        else
                        {
                            _repository.Insert(loanFinancePlan);
                        }
                    }

                    transaction.Commit();
                }

            return syncedLoanFinancePlans;
        }

        public decimal GetAvailBalance(int clientId, AvailBalanceRequest availBalancerequest)
        {
            availBalancerequest.ClientId = clientId;
            return _contractService.GetAvailableBalanceForDAMU(availBalancerequest);
        }

        public class AvailBalanceRequest
        {
            public int ClientId { get; set; }
            public int SettingId { get; set; }
        }
    }
}
