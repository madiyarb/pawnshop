using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.LoanFinancePlans;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts.LoanFinancePlans;
using Pawnshop.Services.Domains;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.Contracts
{
    public class ContractVerificationService : IContractVerificationService
    {
        private readonly ILoanFinancePlanSerivce _loanFinancePlanSerivce;
        private readonly IDomainService _domainService;
        private readonly IContractService _contractService;
        private readonly AccountRepository _accountRepository;
        private readonly ClientRepository _clientRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly IClientSignerService _clientSignerService;
        private readonly LoanSubjectRepository _loanSubjectRepository;
        private readonly IClientService _clientService;
        private readonly NotionalRateRepository _notionalRateRepository;

        public ContractVerificationService(
            ILoanFinancePlanSerivce loanFinancePlanSerivce,
            IDomainService domainService,
            IContractService contractService,
            AccountRepository accountRepository,
            ClientRepository clientRepository,
            LoanPercentRepository loanPercentRepository,
            IClientSignerService clientSignerService,
            LoanSubjectRepository loanSubjectRepository,
            NotionalRateRepository notionalRateRepository,
            IClientService clientService)
        {
            _loanFinancePlanSerivce = loanFinancePlanSerivce;
            _domainService = domainService;
            _contractService = contractService;
            _accountRepository = accountRepository;
            _clientRepository = clientRepository;
            _loanPercentRepository = loanPercentRepository;
            _clientSignerService = clientSignerService;
            _loanSubjectRepository = loanSubjectRepository;
            _notionalRateRepository = notionalRateRepository;
            _clientService = clientService;
        }

        public void ClientAccordanceToProduct(Contract contract)
        {
            if (contract.ProductType.Code.Equals(Constants.PRODUCT_DAMU) && contract.Client.LegalForm.IsIndividual && !contract.Client.LegalForm.Code.Equals(Constants.MICRO_ENTREPRENEUR_NOT_REGISTERED))
                throw new PawnshopApplicationException($"Клиенту {contract.Client.FullName} невозможно выдать займ в рамках продукта ДАМУ");

            if (contract.ProductType.Code.Equals(Constants.PRODUCT_GRANT) && contract.Client.LegalForm.IsIndividual && !contract.Client.LegalForm.Code.Equals(Constants.MICRO_ENTREPRENEUR_NOT_REGISTERED))
                throw new PawnshopApplicationException($"Клиенту {contract.Client.FullName} невозможно выдать займ в рамках продукта Оркендеу");
        }

        //метод сравнивает кол-во записей ФП у которых Цель финансирования совпадает с Целью финансирования Договора
        //для INVESTMENTS + CURRENT_ASSETS все записи ФП должны иметь Цель финансирования полностью совпадающие с Целью Договора
        //для INVESTMENTS_AND_CURRENT_ASSETS цели ФП могут быть перемешаны, поэтому для этой Цели финансирования проверка не срабатывает
        private bool CheckLoanPurposeAccordingFPPurpose(List<LoanFinancePlan> loanFinancePlans, Contract contract)
        {
            if (contract.LoanPurposeId is null)
                throw new PawnshopApplicationException($"По Договору {contract.ContractNumber} цель займа не установлена");

            if (loanFinancePlans.Count == 0)
                throw new PawnshopApplicationException($"По Договору {contract.ContractNumber} в рамках финансирования ДАМУ список Финансового плана пустой");

            var loanPurposeCode = _domainService.getDomainCodeById(contract.LoanPurposeId).Code;

            if (!loanPurposeCode.Equals(Constants.INVESTMENTS_AND_CURRENT_ASSETS))
            {
                List<string> allPurposeListOfFinancePlan = loanFinancePlans.Select(x => _domainService.getDomainCodeById(x.LoanPurposeId).Code.ToString()).ToList();
                int allPurposeListOfFinancePlanCount = allPurposeListOfFinancePlan.Count;
                var machedWithLoanPurposeListCount = allPurposeListOfFinancePlan.FindAll(x => x.Equals(loanPurposeCode)).Count;

                if (allPurposeListOfFinancePlanCount != machedWithLoanPurposeListCount)
                    return false;
            }

            return true;
        }

        public void LoanPurposeAccordanceToFinancePlanPurposes(Contract contract)
        {
            if (contract.Id > 0)
            {
                var loanFinancePlan = _loanFinancePlanSerivce.GetList(contract.Id);
                var isLoanPurposeAccordanceFPPurpose = CheckLoanPurposeAccordingFPPurpose(loanFinancePlan, contract);

                if (contract.ProductType.Code.Equals(Constants.PRODUCT_DAMU) && !isLoanPurposeAccordanceFPPurpose)
                    throw new PawnshopApplicationException($"По Договору {contract.ContractNumber} в рамках продукта ДАМУ цели финансирования ФП не соответствуют цели финансирования по договору");
            }
        }

        public void CheckClientContractsAmountWithProductLoanCost(Contract contract)
        {
            var allClientDAMUContractAmount = _contractService.GetDebtAndDebtOverdueBalanceForClient(contract.ClientId, Constants.PRODUCT_DAMU);
            var loanSettingsForProduct = _loanPercentRepository.Get(contract.SettingId.Value);

            if (allClientDAMUContractAmount > loanSettingsForProduct.LoanCostTo)
                throw new PawnshopApplicationException($"Сумма всех кредитов клиента не должна превышать {loanSettingsForProduct.LoanCostTo.ToString("N0", CultureInfo.CreateSpecificCulture("sv-SE"))} тг");
        }

        private void CheckClientContractsAmountWithProductLoanCost(Contract contract, LoanPercentSetting setting)
        {
            var allClientDAMUContractAmount = _contractService.GetDebtAndDebtOverdueBalanceForClient(contract.ClientId, Constants.PRODUCT_DAMU) + contract.LoanCost;

            if (allClientDAMUContractAmount > setting.LoanCostTo)
                throw new PawnshopApplicationException($"Сумма всех кредитов клиента не должна превышать {setting.LoanCostTo.ToString("N0", CultureInfo.CreateSpecificCulture("sv-SE"))} тг");
        }

        public void CheckDAMUContractMaturityDate(Contract contract)
        {
            var loanSettingsForProduct = _loanPercentRepository.Get(contract.SettingId.Value);
            if (contract.MaturityDate > loanSettingsForProduct.AvailableDateTill)
                throw new PawnshopApplicationException($"Срок возврата кредита не должен быть больше срока погашения кредитной линии по ДАМУ ({loanSettingsForProduct.AvailableDateTill.Value.ToShortDateString()})");
        }

        public void CheckDAMUContract(Contract contract)
        {
            var correctLegalForm = _clientSignerService.GetClientSignersAllowedDocumentTypes(contract.Client.LegalFormId).Any();

            if (!correctLegalForm)
                throw new PawnshopApplicationException($"Тип правовой формы клиента по договору в рамках продукта ДАМУ только ТОО, ИП, КХ, Физ лицо без регистрации ИП");

            if (!contract.LCDate.HasValue)
                throw new PawnshopApplicationException($"Поле \"Дата решения кредитного комитета\" обязательно для заполнения");

            if (string.IsNullOrWhiteSpace(contract.LCDecisionNumber))
                throw new PawnshopApplicationException($"Поле \"Номер решения кредитного комитета\" обязательно для заполнения");

            var loanSettingsForProduct = _loanPercentRepository.Get(contract.SettingId.Value);

            if (contract.MaturityDate > loanSettingsForProduct.AvailableDateTill)
                throw new PawnshopApplicationException($"Срок возврата кредита не должен быть больше срока погашения кредитной линии по ДАМУ ({loanSettingsForProduct.AvailableDateTill.Value.ToShortDateString()})");


            CheckClientContractsAmountWithProductLoanCost(contract, loanSettingsForProduct);

            LoanPurposeAccordanceToFinancePlanPurposes(contract);
        }

        public void CheckGuarantorSubjects(List<ContractLoanSubject> contractSubjects, string? productTypeCode = null)
        {
            var contractGuarantorSubjects = contractSubjects.Where(sub => sub.Subject.Code == Constants.GUARANTOR_CODE);

            // если код продукта предоставлен и является продуктом Керемет или Шапагат, то разрешаем Юр. Лицо быть гарантом
            if (!String.IsNullOrEmpty(productTypeCode) && productTypeCode == Constants.PRODUCT_TMF_REALTY)
                return;
            // если не был предоставлен тип продукта или продукт не является Керемет или Шапагат, автоматически проверяет гарантов на физ.лицо
            else
                if (contractGuarantorSubjects.Any())
                foreach (var subject in contractGuarantorSubjects)
                    if (subject.Client.LegalForm.Code != Constants.INDIVIDUAL)
                        throw new PawnshopApplicationException($"Клиент {subject.Client.FullName} должен быть физ.лицом");
        }

        public void CheckContractPositions(Contract contract)
        {
            foreach (var position in contract.Positions)
            {
                var contractPositionDetails = _contractService.GetContractPositionDetail(position);

                if (contractPositionDetails.IsDisabled)
                    throw new PawnshopApplicationException($"Залогодатель является юр.лицом, но не является Заемщиком");

                if (position.Position.ClientId.HasValue)
                {
                    var isLegalClient = _clientSignerService.GetClientSignersAllowedDocumentTypes(position.Position.Client.LegalFormId).Any();

                    if (isLegalClient)
                        if (position.Position.ClientId != contract.ClientId)
                            throw new PawnshopApplicationException($"Залогодатель является юр.лицом, но не является Заемщиком");
                }
                else
                    throw new PawnshopApplicationException($"Наличие залогодателя обязательно на Позиции договора");
            }
        }

        public void CheckCoborrowerSubjects(List<ContractLoanSubject> contractSubjects, string productTypeCode)
        {
            var contractCoborrowerSubjects = contractSubjects.Where(x => x.Subject.Code == Constants.COBORROWER_CODE);

            if (productTypeCode == Constants.PRODUCT_TMF_REALTY)
            {
                if (contractCoborrowerSubjects.Any())
                {
                    foreach (var subject in contractCoborrowerSubjects)
                    {
                        if (subject.Client.LegalForm.Code != Constants.INDIVIDUAL)
                            throw new PawnshopApplicationException($"Клиент {subject.Client.FullName} должен быть физ.лицом");
                    }
                }
            }

            return;
        }

        public async Task CheckPositionSubjects(Contract contract)
        {
            foreach (var contractPosition in contract.Positions)
            {
                var allPositionSubjects = new List<int>
                    {
                        contractPosition.Position.ClientId.HasValue ? contractPosition.Position.ClientId.Value : throw new PawnshopApplicationException("Клиент не был заполнен для позиции - ContractVerificationService.CheckPositionSubjects")
                    };
                allPositionSubjects.AddRange(contractPosition.Position.PositionSubjects.Select(x => x.ClientId).ToList());
                if (allPositionSubjects.Count() != allPositionSubjects.Distinct().Count())
                {
                    throw new PawnshopApplicationException("Субъект для позиции не должен повторяться");
                }

                if (contractPosition.Position.PositionSubjects != null && contractPosition.Position.PositionSubjects.Any())
                {
                    foreach (var subject in contractPosition.Position.PositionSubjects)
                    {
                        if (subject.SubjectId == null || subject.SubjectId == 0)
                            throw new PawnshopApplicationException("Не выбран тип субъекта для позиции договора");
                        if (subject.ClientId == null || subject.ClientId == 0)
                        {
                            throw new PawnshopApplicationException("Не выбран клиент для субъекта позиции догвора");
                        }
                    }
                }
            }
        }

        public async Task CheckLtvForContractEstimatedCost(Contract contract)
        {

            if (contract.SettingId == null)
                return;

            var loanSettingsForProduct = _loanPercentRepository.Get(contract.SettingId.Value);

            if (loanSettingsForProduct.LTV == null && !loanSettingsForProduct.ProductTypeLTVs.Any())
                return;

            if (loanSettingsForProduct.ProductType.Code != Constants.PRODUCT_TMF_REALTY && loanSettingsForProduct.ProductType.Code != Constants.PRODUCT_DAMU)
                return;

            //проверяем ЛТВ для каждой позиции
            if (loanSettingsForProduct.ProductTypeLTVs.Any())
            {
                decimal totalCollateralCost = 0;

                totalCollateralCost = contract.Positions.Sum(x => x.CollateralCost ?? Decimal.Zero);

                if (totalCollateralCost < contract.LoanCost)
                    throw new PawnshopApplicationException($"Ссуда({contract.LoanCost}) превышает общую доступную залоговую стоимость({totalCollateralCost})");
            }

            //если в продукте настроен ЛТВ, то по нему тоже проверяем
            if (loanSettingsForProduct.LTV == null)
                return;

            var availableLoanCost = contract.EstimatedCost * loanSettingsForProduct.LTV.Value;
            if (contract.LoanCost > availableLoanCost)
                throw new PawnshopApplicationException($"Сумма займа не должна превышать {(int)(loanSettingsForProduct.LTV.Value * 100)}% от Оценки позиций, предоставляемых в залог");
        }

        public async Task CheckPositionEstimateDate(Contract contract)
        {
            foreach (var position in contract.Positions)
                if (position.PositionEstimate?.Date?.AddMonths(Constants.POSITION_ESTIMATE_EXPIRE_MONTH) < DateTime.Today)
                    throw new PawnshopApplicationException($"Оценка позиции не должна превышать срок {Constants.POSITION_ESTIMATE_EXPIRE_MONTH} месяцев");
        }

        public async Task CheckPositionSubjectClients(Contract contract)
        {
            foreach (var position in contract.Positions)
            {

                //если не недвижимость, то пропускаем
                if (position.Position.CollateralType != CollateralType.Realty)
                    continue;

                //если залогодатель - заемщик, то пропускаем
                if (position.Position.ClientId == contract.ClientId)
                    continue;

                //если созологадатель - заемщик, то пропускаем
                if (position.Position.PositionSubjects.Any(x => x.ClientId == contract.ClientId))
                    continue;

                //если заемщик ИП, то проверяем залогодателя/созалогодателя на ИИН (юридически ИП = физ.лицу для ПКБ)
                if (contract.Client.LegalForm.Code == Constants.SOLE_PROPRIETOR)
                {

                    if (position.Position.Client.IdentityNumber == contract.Client.IdentityNumber)
                        continue;

                    if (position.Position.PositionSubjects.Any(x => x.Client.IdentityNumber == contract.Client.IdentityNumber))
                        continue;
                }

                //если залогодатель/созалогодатель - созаемщик или гарант, то пропускаем
                var coborrowersAndGuarantors = contract.Subjects?.Where(x => x.Subject.Code == Constants.COBORROWER_CODE || x.Subject.Code == Constants.GUARANTOR_CODE).ToList();

                if (coborrowersAndGuarantors != null)
                {
                    if (coborrowersAndGuarantors.Any(x => x.ClientId == position.Position.ClientId))
                        continue;

                    position.Position.PositionSubjects.ForEach(subject =>
                    {
                        subject.Subject = _loanSubjectRepository.Get(subject.SubjectId);
                    });
                    var copledgers = position.Position.PositionSubjects?.Where(x => x.Subject.Code == Constants.PLEDGER_CODE).ToList();

                    if (copledgers != null)
                    {
                        if (copledgers.Any(x => coborrowersAndGuarantors.Any(y => y.ClientId == x.ClientId)))
                            continue;
                    }

                    if (contract.Client.LegalForm.Code == Constants.SOLE_PROPRIETOR)
                    {
                        if (coborrowersAndGuarantors.Any(x => x.Client.IdentityNumber == position.Position.Client.IdentityNumber))
                            continue;

                        if (copledgers.Any(x => coborrowersAndGuarantors.Any(y => y.Client.IdentityNumber == x.Client.IdentityNumber)))
                            continue;
                    }
                }

                //если не удовлетворяет ни одно из условий - ошибка
                throw new PawnshopApplicationException($"Если залогодатель/созалогодатель по позиции {position.Position.Name} не является заемщиком, то залогодатель/созологадатель должен быть Созаемщиком или Гарантом");
            }
        }

        public async Task CheckEIfEstimatedCostPositiveForPositions(Contract contract)
        {
            foreach (var position in contract.Positions)
            {
                if (position.EstimatedCost <= 0)
                    throw new PawnshopApplicationException($"Оценка позиции {position.Position.Name} не может иметь негативное значение");
            }
        }

        public async Task CheckIfRealtyContractConfirmed(Contract contract)
        {
            if (contract.ContractClass == ContractClass.CreditLine || contract.ContractClass == ContractClass.Tranche)
                return;

            if (contract.CollateralType != CollateralType.Realty)
                return;

            if (contract.Status != ContractStatus.Confirmed)
                throw new PawnshopApplicationException("Договор недвижимости не был подтвержден. Подтвердите для подписания");
        }

        public async Task CheckClientsAge(Contract contract)
        {

            if (contract.Client?.LegalForm?.Code == Constants.INDIVIDUAL)
            {
                var clientAge = _clientService.GetClientAge(contract.Client);
                if (clientAge < Constants.CLIENT_MINIMAL_AGE)
                    throw new PawnshopApplicationException("Клиенту на момент выдачи кредита должно быть не менее 21 года");
            }

            if ((contract.Client?.LegalForm?.Code == Constants.SOLE_PROPRIETOR) && contract.Client != null && contract.Client.ChiefId.HasValue)
            {
                var client = _clientService.Get(contract.Client.ChiefId.Value);
                var clientAge = _clientService.GetClientAge(client);
                if (clientAge < Constants.CLIENT_MINIMAL_AGE)
                    throw new PawnshopApplicationException("Клиенту на момент выдачи кредита должно быть не менее 21 года");
            }

            if (contract.Subjects != null && contract.Subjects.Count > 0)
            {
                var coborrowersAndGuarantors = contract.Subjects.Where(x => x.Subject.Code == Constants.COBORROWER_CODE || x.Subject.Code == Constants.GUARANTOR_CODE).ToList();
                foreach (var coborrower in coborrowersAndGuarantors)
                {
                    if (coborrower.Client != null)
                    {
                        var coborrowerAge = _clientService.GetClientAge(coborrower.Client);
                        if (coborrowerAge < Constants.CLIENT_MINIMAL_AGE)
                            throw new PawnshopApplicationException("Созаемщику или гаранту на момент выдачи кредита должно быть не менее 21 года");
                    }
                }
            }
        }

        public async Task CheckClientCoborrowerAccountAmountLimit(Contract contract)
        {
            var domainValue = _domainService.GetDomainValue(
                Constants.NOTIONAL_RATE_TYPES,
                Constants.NOTIONAL_RATE_TYPES_MRP);
            var nationalRate = await _notionalRateRepository.GetByTypeOfLastYearAsync(domainValue.Id) ??
                throw new PawnshopApplicationException($"{Constants.NOTIONAL_RATE_TYPES_MRP} не указан!");
            var client = await _clientRepository.GetOnlyClientAsync(contract.ClientId);

            var coborrowerContractsAccountBalance = await _accountRepository.GetCoborrowerContractsAccountBalance(contract.ClientId);
            var clientActiveContractsAccountBalance = await _accountRepository.GetClientActiveContractsAccountBalance(contract.ClientId);

            var generalAccountAmount = coborrowerContractsAccountBalance + clientActiveContractsAccountBalance;

            var generalAmount = generalAccountAmount + contract.LoanCost;

            var availableLoanCost = nationalRate.RateValue * Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP - generalAccountAmount;

            if (generalAccountAmount > nationalRate.RateValue * Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP)
            {
                throw new PawnshopApplicationException(
                    $"По Субъекту ИИН/БИН {client.IdentityNumber}, сумма ОД {generalAmount} превышают {Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP} МРП. Доступная сумма 0 тенге");
            }

            if (generalAmount > nationalRate.RateValue * Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP)
            {
                throw new PawnshopApplicationException(
                    $"По Субъекту ИИН/БИН {client.IdentityNumber}, сумма ОД {generalAmount} превышают {Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP} МРП. Доступная сумма {availableLoanCost} тенге");
            }
        }
    }
}