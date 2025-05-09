using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Data.Models.LegalCollection.PrintTemplates;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionPrintTemplateService : ILegalCollectionPrintTemplateService
    {
        private readonly IContractService _contractService;
        private readonly ILegalCollectionRepository _legalCollectionsRepository;
        private readonly IInscriptionService _inscriptionService;
        private readonly IUserBranchSignerRepository _signerRepository;
        private readonly UserRepository _usersRepository;
        private readonly AccountRepository _accountRepository;
        private readonly AccountService _accountService;
        private readonly ILegalCollectionRepository _legalCollections;
        private readonly ILegalCollectionPrintTemplateHttpService _legalCollectionPrintTemplateHttpService;
        private readonly RealtyAddressRepository _realtyAddressRepository;
        private readonly UserRepository _users;
        private readonly CollectionStatusRepository _collectionStatusRepository;
        private readonly GroupRepository _groupRepository;

        public LegalCollectionPrintTemplateService(
            IContractService contractService,
            ILegalCollectionRepository legalCollectionsRepository,
            IInscriptionService inscriptionService,
            IUserBranchSignerRepository signerRepository,
            UserRepository usersRepository,
            AccountRepository accountRepository,
            AccountService accountService,
            ILegalCollectionPrintTemplateHttpService legalCollectionPrintTemplateHttpService,
            ILegalCollectionRepository legalCollections,
            RealtyAddressRepository realtyAddressRepository,
            UserRepository users,
            CollectionStatusRepository collectionStatusRepository,
            GroupRepository groupRepository)
        {
            _contractService = contractService;
            _legalCollectionsRepository = legalCollectionsRepository;
            _inscriptionService = inscriptionService;
            _signerRepository = signerRepository;
            _usersRepository = usersRepository;
            _accountRepository = accountRepository;
            _accountService = accountService;
            _legalCollectionPrintTemplateHttpService = legalCollectionPrintTemplateHttpService;
            _legalCollections = legalCollections;
            _realtyAddressRepository = realtyAddressRepository;
            _users = users;
            _collectionStatusRepository = collectionStatusRepository;
            _groupRepository = groupRepository;
        }
        
        public async Task<LegalCasePrintTemplateModel> GetAsync(LegalCasePrintTemplateQuery query)
        {
            switch (query.PrintTemplateId)  
            {
                case (int)LegalCasePrintTemplateEnum.CLAIM_RECOVERY:
                {
                    var signer = await GetPrintTemplateSignerAsync(query.ContractId);
                    var model = await _legalCollectionsRepository.GetPrintFormDataAsync(query.ContractId);
                    model.ContractData = GetContractCosts(model.ContractData);
                    model.ContractData.NominalRate = Math.Round(model.ContractData.LoanPercent * 360, 2);
                    model.ContractData.DebtCost = Math.Ceiling(model.ContractData.DebtCost);
                    model.Signer = signer?.Fullname;
                    model.PhoneNumber = signer?.PhoneNumber;
                    model.ContractData.CoborrowerList = (await _legalCollections
                    .GetPrintTemplateSubjectList(query.ContractId, Constants.COBORROWER_CODE)).ToList();

                    model.ContractData.GuarantorList = (await _legalCollections
                    .GetPrintTemplateSubjectList(query.ContractId, Constants.GUARANTOR_CODE)).ToList();

                    return model;
                }
                case (int)LegalCasePrintTemplateEnum.APPLICATION_WRITE_EXECUTION:
                {
                    var signer = await GetPrintTemplateSignerAsync(query.ContractId);
                    var model = await _legalCollectionsRepository.GetPrintFormDataAsync(query.ContractId);
                    model.ContractData.NominalRate = Math.Round(model.ContractData.LoanPercent * 360, 2);
                    model.ContractData = GetContractCosts(model.ContractData);
                    model.ContractData.DebtCost = Math.Ceiling(model.ContractData.DebtCost);
                    model.Signer = signer?.Fullname;
                    model.PhoneNumber = signer?.PhoneNumber;
                    model.ContractData.CoborrowerList = (await _legalCollections
                    .GetPrintTemplateSubjectList(query.ContractId, Constants.COBORROWER_CODE)).ToList();

                    model.ContractData.GuarantorList = (await _legalCollections
                        .GetPrintTemplateSubjectList(query.ContractId, Constants.GUARANTOR_CODE)).ToList();

                    return model;
                }
                case (int)LegalCasePrintTemplateEnum.DEBT_CALCULATION:
                {
                    Contract contract = _contractService.Get(query.ContractId);
                    var inscription = contract?.InscriptionId != null
                        ? await _inscriptionService.GetOnlyInscriptionAsync((int)contract.InscriptionId)
                        : null;
                    string totalCost = null;
                    if (inscription != null)
                    {
                        totalCost = GetCostInStringFormat(inscription.TotalCost);
                    }

                    var model = await _legalCollectionsRepository.GetPrintFormDataAsync(query.ContractId);

                    model.ContractData.NominalRate = Math.Round(model.ContractData.LoanPercent * 360, 2);
                    model.ContractData = GetContractCosts(model.ContractData);
                    model.DebtCalculationPrintForm = new DebtCalculationPrintForm
                    {
                        ContractNumber = contract.ContractNumber,
                        ContractDate = contract.ContractDate,
                        Amounts = await GetCalculateDebtPrintFormAmounts(query.ContractId, contract),
                        DebtDate = inscription?.Date.ToString("dd.MM.yyyy"),
                        TotalCost = totalCost,
                        DelayDays = contract.DelayDays,
                        BranchName = contract.Branch.DisplayName
                    };

                    return model;
                }
                case (int)LegalCasePrintTemplateEnum.LETTER_BEFORE_CLAIM:
                {
                    int creditLineId = 0;
                    var model = await _legalCollections.GetPrintFormDataAsync(query.ContractId);
                    model.ContractData.CoborrowerList =
                        (await _legalCollections.GetPrintTemplateSubjectList(query.ContractId,
                            Constants.COBORROWER_CODE)).ToList();
                    model.ContractData.GuarantorList =
                        (await _legalCollections.GetPrintTemplateSubjectList(query.ContractId,
                            Constants.GUARANTOR_CODE)).ToList();
                    var realtyList = await _realtyAddressRepository.GetByContractIdAsync(query.ContractId);
                    if (!realtyList.Any())
                    {
                        creditLineId = await _contractService.GetCreditLineId(query.ContractId);
                        realtyList = await _realtyAddressRepository.GetByContractIdAsync(creditLineId);
                    }

                    model.ContractData.RealtyAddressList =
                        realtyList == null ? null : realtyList.Select(x => x.FullPathRus).ToList();
                    var carDataList = await _legalCollections.GetPrintTemplateCarPositionList(query.ContractId);
                    if (!carDataList.Any())
                    {
                        if (creditLineId == 0)
                        {
                            creditLineId = await _contractService.GetCreditLineId(query.ContractId);
                        }

                        carDataList = await _legalCollections.GetPrintTemplateCarPositionList(creditLineId);
                    }
                    var signer = await GetPrintTemplateSignerAsync(query.ContractId);
                    model.ContractData.CarDataList = carDataList.ToList();
                    model.ContractData = GetContractCosts(model.ContractData);
                    model.ContractData.NominalRate = Math.Round(model.ContractData.LoanPercent * 360, 2);
                    model.ContractData.DebtCost = Math.Ceiling(model.ContractData.DebtCost);
                    model.Signer = signer?.Fullname; 
                    model.PhoneNumber = signer?.PhoneNumber;
                    var collection = await _collectionStatusRepository.GetByContractIdAsync(query.ContractId);
                    model.ContractData.DelayDays = collection is null
                        ? 0
                        : DateTime.Now.Date.Subtract(collection.StartDelayDate.Date).Days;
                    
                    model.CompanyData.ActualAddress = _groupRepository.Find(new { Name = "ZHK" }).Configuration
                        .ContactSettings.Address;

                    return model;
                }
            }

            return null;
        }

        public async Task<List<LegalCasePrintTemplate>> GetListAsync(int contractId)
        {
            var result = await GetLegalCasePrintTemplateList();
            if (!result.Any())
            {
                return new List<LegalCasePrintTemplate>();
            }

            var contract = await _contractService.GetOnlyContractAsync(contractId);
            if (!contract.InscriptionId.HasValue)
            {
                result = result.Where(t => t.PrintTemplateId != (int)LegalCasePrintTemplateEnum.DEBT_CALCULATION)
                    .ToList();
            }

            foreach (var item in result)
            {
                item.PrintTemplateCode = ((LegalCasePrintTemplateEnum)item.PrintTemplateId).ToString();
            }

            return result;
        }


        private LegalCasePrintTemplateContractData GetContractCosts(LegalCasePrintTemplateContractData contractData)
        {
            var contractBalances = _contractService
            .GetBalances(new List<int> { contractData.ContractId }).FirstOrDefault();

            contractData.LoanAmountCost = contractBalances.AccountAmount;
            contractData.OverdueDebtCost = contractBalances.OverdueAccountAmount;
            contractData.OverdueLoanCost = contractBalances.OverdueProfitAmount;
            contractData.PenaltyCost = contractBalances.PenyAmount;
            contractData.ProfitAmountCost = contractBalances.ProfitAmount;
            contractData.DebtCost = contractData.LoanAmountCost +
                                    contractData.OverdueDebtCost +
                                    contractData.OverdueLoanCost +
                                    contractData.PenaltyCost +
                                    contractData.ProfitAmountCost;
            return contractData;
        }

        private async Task<User> GetPrintTemplateSignerAsync(int contractId)
        {
            var contract = await _contractService.GetOnlyContractAsync(contractId);
            if (contract is null)
            {
                return null;
            }

            var branch = await GetGroup(contract.BranchId, contract.Id);
            if (branch is null)
            {
                return null;
            }

            var userBranchSigner = await _signerRepository.GetByBranch(branch.Id);
            if (userBranchSigner is null)
            {
                return null;
            }

            var user = await _usersRepository.GetAsync(userBranchSigner.UserId);
            return user;
        }

        private string GetCostInStringFormat(decimal cost)
        {
            CultureInfo culture = new CultureInfo("ru-RU");
            if (cost % 1 == 0)
            {
                return cost.ToString("N0", culture);
            }

            return cost.ToString("N2", culture);
        }

        private async Task<List<ContractAmountsInfoDto>> GetCalculateDebtPrintFormAmounts(int contractId,
            Contract contract)
        {
            var contractBalance = await _accountRepository.GetBalanceByContractIdAsync(contractId);
            var pennyAccountId = await _legalCollectionsRepository.GetPennyAccountIdByContractIdAsync(contractId);
            var profitAccountId = await _legalCollectionsRepository.GetPennyProfitIdByContractIdAsync(contractId);

            var accountPenny = await _accountService.GetAccountBalanceAsync(pennyAccountId, DateTime.Now);
            var profitPenny = await _accountService.GetAccountBalanceAsync(profitAccountId, DateTime.Now);
            var loanPercent = contract.LoanPercent.ToString("0.####");

            return new List<ContractAmountsInfoDto>
            {
                new ContractAmountsInfoDto
                {
                    Name = "Сумма займа",
                    Cost = contract.LoanCost,
                    StringCost = GetCostInStringFormat(contract.LoanCost)
                },
                new ContractAmountsInfoDto
                {
                    Name = "Основной долг",
                    Cost = contractBalance.TotalAcountAmount,
                    StringCost = GetCostInStringFormat(contractBalance.TotalAcountAmount)
                },

                // Проценты начисленные + Проценты просроченные
                new ContractAmountsInfoDto
                {
                    Name = $"Вознаграждение, {loanPercent}",
                    Cost = contractBalance.ProfitAmount + contractBalance.OverdueProfitAmount,
                    StringCost =
                        GetCostInStringFormat(contractBalance.ProfitAmount + contractBalance.OverdueProfitAmount)
                },

                // Пеня на долг просроченный + Пеня на проценты просроченные.
                new ContractAmountsInfoDto
                {
                    Name = "Просроченное вознаграждение",
                    Cost = accountPenny + profitPenny,
                    StringCost = GetCostInStringFormat(accountPenny + profitPenny)
                }
            };
        }

        private async Task<List<LegalCasePrintTemplate>> GetLegalCasePrintTemplateList()
        {
            var printTemplates = await _legalCollectionPrintTemplateHttpService.List();

            if (printTemplates is null)
            {
                throw new PawnshopApplicationException($"Print Templates не найдены");
            }

            return printTemplates;
        }

        private async Task<Group> GetGroup(int branchId, int contractId)
        {
            var group = await _groupRepository.GetOnlyBranchAsync(branchId);
            if (group.IsTasOnlineBranch())
            {
                return await _groupRepository.GetOnLineGroupAsync(contractId);
            }

            return group;
        }

    }
}