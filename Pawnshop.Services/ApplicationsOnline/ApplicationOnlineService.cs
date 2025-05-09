using Microsoft.Extensions.Options;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineInsurances;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.ApplicationsOnlineCar;
using Pawnshop.Data.Models.ApplicationsOnlineEstimation;
using Pawnshop.Data.Models.Base;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.CreditLines;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Models.Contracts;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.ApplicationsOnline
{
    public sealed class ApplicationOnlineService : IApplicationOnlineService
    {
        private readonly AbsOnlineService _absOnlineService;
        private readonly ApplicationOnlineCarRepository _applicationOnlineCarRepository;
        private readonly ApplicationOnlineInsuranceRepository _applicationOnlineInsuranceRepository;
        private readonly ApplicationOnlinePositionRepository _applicationOnlinePositionRepository;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly ApplicationsOnlineEstimationRepository _applicationsOnlineEstimationRepository;
        private readonly ICarService _carService;
        private readonly IClientService _clientService;
        private readonly ContractAdditionalInfoRepository _contractAdditionalInfoRepository;
        private readonly ContractCheckRepository _contractCheckRepository;
        private readonly ContractCheckValueRepository _contractCheckValueRepository;
        private readonly ContractService _contractService;
        private readonly GroupRepository _groupRepository;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly ILogger _logger;
        private readonly ICreditLineService _creditLineService;
        private readonly EnviromentAccessOptions _options;

        public ApplicationOnlineService(
            AbsOnlineService absOnlineService,
            ApplicationOnlineCarRepository applicationOnlineCarRepository,
            ApplicationOnlineInsuranceRepository applicationOnlineInsuranceRepository,
            ApplicationOnlinePositionRepository applicationOnlinePositionRepository,
            ApplicationOnlineRepository applicationOnlineRepository,
            ApplicationsOnlineEstimationRepository applicationsOnlineEstimationRepository,
            ICarService carService,
            IClientService clientService,
            ContractAdditionalInfoRepository contractAdditionalInfoRepository,
            ContractCheckRepository contractCheckRepository,
            ContractCheckValueRepository contractCheckValueRepository,
            ContractService contractService,
            GroupRepository groupRepository,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            LoanPercentRepository loanPercentRepository,
            ILogger logger,
            ICreditLineService creditLineService,
            IOptions<EnviromentAccessOptions> options)
        {
            _absOnlineService = absOnlineService;
            _applicationOnlineCarRepository = applicationOnlineCarRepository;
            _applicationOnlineInsuranceRepository = applicationOnlineInsuranceRepository;
            _applicationOnlinePositionRepository = applicationOnlinePositionRepository;
            _applicationOnlineRepository = applicationOnlineRepository;
            _applicationsOnlineEstimationRepository = applicationsOnlineEstimationRepository;
            _carService = carService;
            _clientService = clientService;
            _contractAdditionalInfoRepository = contractAdditionalInfoRepository;
            _contractCheckRepository = contractCheckRepository;
            _contractCheckValueRepository = contractCheckValueRepository;
            _contractService = contractService;
            _groupRepository = groupRepository;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _loanPercentRepository = loanPercentRepository;
            _logger = logger;
            _creditLineService = creditLineService;
            _options = options.Value;
        }

        public async Task ChangeDetailForInsurance(ApplicationOnline applicationOnline, int authorId, int? productId = null, decimal? newApplicationAmount = null, decimal? estimateAmount = null)
        {
            var creditLineLimit = await GetCreditLineLimit(applicationOnline.CreditLineId);
            var productWithInsurance = await GetProduct(productId ?? applicationOnline.ProductId, true);
            var productWithoutInsurance = await GetProduct(productId ?? applicationOnline.ProductId, false);
            var insurance = await _applicationOnlineInsuranceRepository.GetByApplicationId(applicationOnline.Id);

            decimal? estimateMaxAmount;

            if (estimateAmount.HasValue)
            {
                if (applicationOnline.CreditLineId.HasValue)
                {
                    var creditLineBalance = _contractService.GetCreditLineTotalBalance(applicationOnline.CreditLineId.Value).Result;
                    var maxAmount = estimateAmount - creditLineBalance.AccountAmount;

                    estimateMaxAmount = maxAmount > 0 ? maxAmount : 0;
                }
                else
                {
                    estimateMaxAmount = estimateAmount;
                }
            }
            else
            {
                estimateMaxAmount = GetEstimateMaxAmount(applicationOnline.Id, applicationOnline.CreditLineId);
            }

            if (insurance == null)
            {
                applicationOnline.ProductId = productWithoutInsurance?.Id ?? applicationOnline.ProductId;

                if (newApplicationAmount.HasValue)
                {
                    var maxApplicationAmount = GetInternalMaxApplicationAmount(creditLineLimit, estimateMaxAmount, productWithoutInsurance.LoanCostTo);

                    if (newApplicationAmount.Value > maxApplicationAmount)
                        applicationOnline.ApplicationAmount = maxApplicationAmount;
                    else
                        applicationOnline.ApplicationAmount = newApplicationAmount.Value;
                }

                if (estimateAmount.HasValue && estimateAmount.Value < applicationOnline.ApplicationAmount)
                    applicationOnline.ApplicationAmount = estimateAmount.Value;

                return;
            }

            var needChange = false;

            if (productId.HasValue && productId.Value != applicationOnline.ProductId)
                needChange = true;

            if (newApplicationAmount.HasValue && !(newApplicationAmount.Value == insurance.AmountForCustomer || newApplicationAmount.Value == insurance.TotalLoanAmount))
                needChange = true;

            if (estimateAmount.HasValue && estimateAmount.Value < insurance.TotalLoanAmount)
                needChange = true;

            if (estimateMaxAmount != null)
                if (estimateAmount != 0)
                    if (insurance.TotalLoanAmount > estimateMaxAmount)
                        needChange = true;

            if (!needChange)
                return;

            await DeleteInsurance(applicationOnline, insurance, authorId);

            if (productWithInsurance != null)
            {
                var maxApplicationAmount = GetInternalMaxApplicationAmount(creditLineLimit, estimateMaxAmount, productWithInsurance.LoanCostTo);
                var maxApplicationAmountWithoutPremium = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(maxApplicationAmount);
                var amount = insurance.AmountForCustomer;

                if (amount > maxApplicationAmountWithoutPremium)
                    amount = maxApplicationAmountWithoutPremium;

                if (newApplicationAmount.HasValue && newApplicationAmount.Value > maxApplicationAmountWithoutPremium)
                    newApplicationAmount = maxApplicationAmountWithoutPremium;

                await CreateInsurance(applicationOnline, newApplicationAmount ?? amount, productWithInsurance, authorId);
                applicationOnline.ProductId = productWithoutInsurance?.Id ?? productWithInsurance.Id;
            }
            else
            {
                var maxApplicationAmount = GetInternalMaxApplicationAmount(creditLineLimit, estimateMaxAmount, productWithInsurance.LoanCostTo);
                var amount = applicationOnline.ApplicationAmount;

                if (amount > maxApplicationAmount)
                    amount = maxApplicationAmount;

                if (newApplicationAmount.HasValue && newApplicationAmount.Value > maxApplicationAmount)
                    newApplicationAmount = maxApplicationAmount;

                applicationOnline.ApplicationAmount = newApplicationAmount ?? amount;
                applicationOnline.ProductId = productWithoutInsurance.Id;
            }
        }

        public async Task CreateContract(ApplicationOnline applicationOnline, int? branchId = null)
        {
            using (var transaction = _loanPercentRepository.BeginTransaction())
            {
                try
                {
                    var branch = branchId.HasValue ? _groupRepository.Get(branchId.Value) : _groupRepository.Find(new { Name = "TSO" });

                    var maturityDate = DateTime.Today.AddMonths(applicationOnline.LoanTerm);
                    var periodTypeId = _contractService.GetPeriodTypeId(maturityDate);
                    var client = _clientService.Get(applicationOnline.ClientId);

                    if (applicationOnline.FirstPaymentDate.HasValue)
                        maturityDate = applicationOnline.FirstPaymentDate.Value.AddMonths(applicationOnline.LoanTerm - 1);

                    var creditLine = CreateCreditLine(applicationOnline, client, periodTypeId, branch.Id, branch.Name);
                    SetContractChecks(creditLine);

                    var parentBranchCode = branch.Id == creditLine.BranchId ? branch.Name : _groupRepository.Get(creditLine.BranchId).Name;

                    var tranche = await CreateTranche(applicationOnline, client, maturityDate, periodTypeId, branch.Id, parentBranchCode, creditLine);
                    SetContractChecks(tranche);

                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, exception.Message);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task CreateInsurance(ApplicationOnline applicationOnline, decimal applicationAmount, LoanPercentSetting product, int authorId)
        {
            if (applicationAmount > product.LoanCostTo)
                applicationAmount = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(product.LoanCostTo);

            var calcInsurance = _insurancePremiumCalculator.GetInsuranceDataV2(applicationAmount, product.InsuranceCompanies.FirstOrDefault().InsuranceCompanyId, product.Id);
            applicationAmount = calcInsurance.Eds == 0 || applicationAmount > 3909999 ? applicationAmount : calcInsurance.Eds;

            var insurance = new ApplicationOnlineInsurance(Guid.NewGuid(), applicationOnline.Id, calcInsurance.InsurancePremium, applicationAmount, applicationAmount + calcInsurance.InsurancePremium);
            await _applicationOnlineInsuranceRepository.Insert(insurance);

            applicationOnline.ApplicationAmount = applicationAmount + calcInsurance.InsurancePremium;
            applicationOnline.LastChangeAuthorId = authorId;
            applicationOnline.UpdateDate = DateTime.Now;
            await _applicationOnlineRepository.Update(applicationOnline);
        }

        public void DeleteDraftContractEntities(int? contractId)
        {
            if (!contractId.HasValue)
                return;

            var contract = _contractService.GetOnlyContract(contractId.Value);

            if (contract == null || contract.Status != ContractStatus.Draft)
                return;

            _absOnlineService.DeleteInsuranceRecords(contractId.Value);
            _contractService.Delete(contractId.Value);
        }

        public async Task DeleteInsurance(ApplicationOnline applicationOnline, ApplicationOnlineInsurance insurance, int authorId)
        {
            insurance.Delete();
            await _applicationOnlineInsuranceRepository.Update(insurance);

            var applicationAmount = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(applicationOnline.ApplicationAmount);

            applicationOnline.ApplicationAmount = applicationAmount;
            applicationOnline.LastChangeAuthorId = authorId;
            applicationOnline.UpdateDate = DateTime.Now;
            await _applicationOnlineRepository.Update(applicationOnline);
        }

        public async Task<decimal> GetMaxApplicationAmount(Guid applicationOnlineId, ApplicationOnline applicationOnline = null)
        {
            if (applicationOnline == null)
                applicationOnline = _applicationOnlineRepository.Get(applicationOnlineId);

            var creditLineLimit = await GetCreditLineLimit(applicationOnline.CreditLineId);
            var estimateAmount = GetEstimateMaxAmount(applicationOnlineId, applicationOnline.CreditLineId);
            var insurance = _applicationOnlineInsuranceRepository.Get(applicationOnlineId);
            var product = await GetProduct(applicationOnline.ProductId, insurance == null ? false : true);
            return GetInternalMaxApplicationAmount(creditLineLimit, estimateAmount, product.LoanCostTo);
        }

        public async Task<LoanPercentSetting> GetProduct(int productId, bool? insurance = null)
        {
            try
            {
                var currentProduct = _loanPercentRepository.Get(productId);

                if (currentProduct == null)
                    return null;

                if (!currentProduct.ParentId.HasValue)
                    return currentProduct;

                if (!insurance.HasValue)
                    return currentProduct;

                var childs = await _loanPercentRepository.GetChild(currentProduct.ParentId.Value);

                return childs.FirstOrDefault(x =>
                    x.IsActual &&
                    x.IsInsuranceAvailable == insurance &&
                    x.ScheduleType == currentProduct.ScheduleType &&
                    x.UseSystemType != UseSystemType.OFFLINE);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return null;
            }
        }

        public TrancheLimit GetAddtionalLimitForInsurance(int contractId)
        {
            var contract = _contractService.GetOnlyContract(contractId);
            if (contract.ContractClass == ContractClass.CreditLine || contract.ContractClass == ContractClass.Tranche)
            {
                var additionalLimit = _creditLineService.GetLimitForInsuranceByPosition(contract.EstimatedCost).Result;

                return new TrancheLimit()
                {
                    MaxSumWithLimit = contract.LoanCost,
                    MaxTrancheSum = contract.LoanCost - additionalLimit,
                    AdditionalLimit = additionalLimit
                };
            }
            return new TrancheLimit();
        }

        public async Task SendInsurance(int contractId)
        {
            var application = _applicationOnlineRepository.GetByContractId(contractId);

            if (application == null || !application.IsCashIssue)
                return;

            _absOnlineService.RegisterPolicy(contractId);
        }

        public async Task CheckEncumbranceRegisteredForCashIssue(int contractId)
        {
            var application = _applicationOnlineRepository.GetByContractId(contractId);

            if (application != null && application.IsCashIssue && !application.EncumbranceRegistered)
                throw new PawnshopApplicationException("Залог не отправлен на регистрацию!");
        }


        private Contract CreateCreditLine(ApplicationOnline applicationOnline, Client client, int periodTypeId, int branchId, string branchCode)
        {
            if (applicationOnline.CreditLineId.HasValue)
                return _contractService.GetOnlyContract(applicationOnline.CreditLineId.Value);

            var product = _loanPercentRepository.GetParent(applicationOnline.ProductId);

            if (product == null)
                throw new Exception($"Не удалось найти продукт для создания кредитной линии по коду {applicationOnline.ProductId}");

            var maxPeriod = (product.ContractPeriodTo * (int?)product.ContractPeriodToType) / (int)product.PaymentPeriodType;

            if (!maxPeriod.HasValue || maxPeriod == 0)
                throw new Exception($"Не удалось получить максимальный срок кредитной линии в настройках продукта {product.Id}");

            var maturityDate = DateTime.Today.AddMonths(maxPeriod.Value);

            if (applicationOnline.FirstPaymentDate.HasValue && maxPeriod.Value <= applicationOnline.LoanTerm &&
                applicationOnline.FirstPaymentDate.Value.AddMonths(applicationOnline.LoanTerm - 1) > DateTime.Now.AddMonths(maxPeriod.Value))
                maturityDate = applicationOnline.FirstPaymentDate.Value.AddMonths(applicationOnline.LoanTerm - 1);

            var applicationOnlinePosition = _applicationOnlinePositionRepository.Get(applicationOnline.ApplicationOnlinePositionId);

            var loanCostWithAdditionalLimit = applicationOnlinePosition.LoanCost.Value + _creditLineService.GetLimitForInsuranceByPosition(applicationOnlinePosition.EstimatedCost.Value).Result;

            var creditLine = new Contract
            {
                AttractionChannelId = applicationOnline.AttractionChannelId,
                AuthorId = applicationOnline.LastChangeAuthorId,
                BranchId = branchId,
                ClientId = applicationOnline.ClientId,
                CollateralType = applicationOnlinePosition.CollateralType,
                ContractClass = product.ContractClass,
                ContractDate = DateTime.Now,
                ContractTypeId = product.ContractTypeId,
                EstimatedCost = Convert.ToInt32(applicationOnlinePosition.EstimatedCost.Value),
                LoanCost = loanCostWithAdditionalLimit,
                LoanPercentCost = Math.Round(loanCostWithAdditionalLimit * product.LoanPercent / 100, 4, MidpointRounding.AwayFromZero),
                LoanPeriod = maxPeriod.Value * 30,
                LoanPurposeId = applicationOnline.LoanPurposeId,
                BusinessLoanPurposeId = applicationOnline.BusinessLoanPurposeId,
                MaturityDate = maturityDate,
                MaxCreditLineCost = loanCostWithAdditionalLimit,
                MinimalInitialFee = 0,
                OriginalMaturityDate = maturityDate,
                OwnerId = branchId,
                PercentPaymentType = PercentPaymentType.Product,
                PeriodTypeId = periodTypeId,
                ProductTypeId = product.ProductTypeId,
                RequiredInitialFee = 0,
                SettingId = product.Id,
                UsePenaltyLimit = product.UsePenaltyLimit,
                ContractData = new ContractData { Client = client },
                ContractSpecific = new GoldContractSpecific { },
                CreatedInOnline = true
            };

            creditLine.ContractNumber = _contractService.GenerateContractNumber(creditLine.ContractDate, branchId, branchCode);

            var applicationOnlineCar = _applicationOnlineCarRepository.Get(applicationOnlinePosition.Id);

            // Машина уже оформлена
            if (applicationOnlineCar.CarId.HasValue)
            {
                creditLine.Positions.Add(new ContractPosition
                {
                    CategoryId = 1,
                    EstimatedCost = Convert.ToInt32(applicationOnlinePosition.EstimatedCost.Value),//TODO тачка проходит переоценку по идее эту сумму и оставляем 
                    LoanCost = loanCostWithAdditionalLimit,
                    PositionCount = 1,
                    PositionId = applicationOnlineCar.CarId.Value,
                });
            }
            else
            {
                creditLine.Positions.Add(new ContractPosition
                {
                    CategoryId = 1,
                    EstimatedCost = Convert.ToInt32(applicationOnlinePosition.EstimatedCost.Value),
                    LoanCost = loanCostWithAdditionalLimit,
                    PositionCount = 1,
                    PositionId = SaveCar(client.Id, applicationOnlineCar),
                });
            }

            _contractService.Save(creditLine);

            var contractAddInfo = new ContractAdditionalInfo
            {
                Id = creditLine.Id,
                SelectedBranchId = applicationOnline.BranchId,
            };

            _contractAdditionalInfoRepository.Insert(contractAddInfo);

            applicationOnline.CreditLineId = creditLine.Id;

            return creditLine;
        }

        private async Task<Contract> CreateTranche(ApplicationOnline applicationOnline, Client client, DateTime maturityDate, int periodTypeId, int branchId, string branchCode, Contract creditLine)
        {
            if (applicationOnline.ContractId.HasValue)
                return _contractService.GetOnlyContract(applicationOnline.ContractId.Value);

            var insurance = await _applicationOnlineInsuranceRepository.GetByApplicationId(applicationOnline.Id);

            var product = await GetProduct(applicationOnline.ProductId, insurance != null);

            var contract = new Contract
            {
                AttractionChannelId = applicationOnline.AttractionChannelId,
                AuthorId = applicationOnline.LastChangeAuthorId,
                BranchId = branchId,
                ClientId = applicationOnline.ClientId,
                CollateralType = creditLine.CollateralType,
                ContractClass = product.ContractClass,
                ContractDate = DateTime.Now,
                ContractTypeId = product.ContractTypeId,
                CreditLineId = creditLine.Id,
                EstimatedCost = Convert.ToInt32(creditLine.EstimatedCost),
                FirstPaymentDate = applicationOnline.FirstPaymentDate,
                LoanCost = applicationOnline.ApplicationAmount,
                LoanPercent = product.LoanPercent,
                LoanPercentCost = Math.Round(applicationOnline.ApplicationAmount * product.LoanPercent / 100, 4, MidpointRounding.AwayFromZero),
                LoanPeriod = Math.Abs((DateTime.Now - maturityDate).Days),
                LoanPurposeId = applicationOnline.LoanPurposeId,
                BusinessLoanPurposeId = applicationOnline.BusinessLoanPurposeId,
                MaturityDate = maturityDate,
                MaxCreditLineCost = creditLine.MaxCreditLineCost,
                MinimalInitialFee = 0,
                OriginalMaturityDate = maturityDate,
                OwnerId = branchId,
                PercentPaymentType = PercentPaymentType.Product,
                PeriodTypeId = periodTypeId,
                ProductTypeId = product.ProductTypeId,
                RequiredInitialFee = 0,
                SettingId = product.Id,
                UsePenaltyLimit = product.UsePenaltyLimit,
                ContractData = new ContractData { Client = client },
                ContractSpecific = new GoldContractSpecific { },
                CreatedInOnline = true
            };

            if (contract.ContractClass == ContractClass.Tranche)
            {
                var tranchesCount = _contractService.GetTranchesCount(contract.CreditLineId.Value).Result + 1;
                contract.ContractNumber = creditLine.ContractNumber + "-T" + tranchesCount.ToString().PadLeft(3, '0');
            }
            else
            {
                contract.ContractNumber = _contractService.GenerateContractNumber(creditLine.ContractDate, branchId, branchCode);
            }

            contract.ContractRates = product.LoanSettingRates
                .Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT || x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT)
                .Select(x => new ContractRate
                {
                    AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                    CreateDate = DateTime.Now,
                    Date = DateTime.Now,
                    Rate = x.Rate,
                    RateSettingId = x.RateSettingId,
                }).ToList();

            _contractService.Save(contract);

            var contractAddInfo = new ContractAdditionalInfo
            {
                Id = contract.Id,
                SelectedBranchId = applicationOnline.BranchId,
                StorageListId = applicationOnline.ListId,
                LoanStorageFileId = applicationOnline.SignType == ApplicationOnlineSignType.NPCK ?
                    Guid.NewGuid() : (Guid?)null,
            };

            _contractAdditionalInfoRepository.Insert(contractAddInfo);

            applicationOnline.ContractId = contract.Id;

            return contract;
        }

        private async Task<decimal?> GetCreditLineLimit(int? creditLineId)
        {
            if (!creditLineId.HasValue)
                return null;

            var creditLine = _contractService.GetOnlyContract(creditLineId.Value);

            if (creditLine.Status != ContractStatus.Signed)
                return null;

            return await _contractService.GetCreditLineLimit(creditLineId.Value);
        }

        private decimal? GetEstimateMaxAmount(Guid applicationOnlineId, int? creditLineId)
        {
            var lastEstimate = _applicationsOnlineEstimationRepository.GetLastByApplicationId(applicationOnlineId);

            if (lastEstimate != null && lastEstimate.Status == nameof(ApplicationOnlineEstimationStatus.Approved) && lastEstimate.IssuedAmount.HasValue)
            {
                if (creditLineId.HasValue)
                {
                    var creditLineBalance = _contractService.GetCreditLineTotalBalance(creditLineId.Value).Result;
                    var maxAmount = (lastEstimate.IssuedAmount ?? 0) - creditLineBalance.AccountAmount;

                    if (maxAmount > 0)
                        return maxAmount;
                    else
                        return 0;
                }

                return lastEstimate.IssuedAmount;
            }

            return null;
        }

        private decimal GetInternalMaxApplicationAmount(decimal? creditLineLimit, decimal? estimateMaxAmount, decimal productMaxAmount)
        {
            var maxApplicationAmount = productMaxAmount;

            if (creditLineLimit.HasValue && creditLineLimit.Value < maxApplicationAmount)
                maxApplicationAmount = creditLineLimit.Value;

            if (estimateMaxAmount.HasValue && estimateMaxAmount.Value < maxApplicationAmount)
                maxApplicationAmount = estimateMaxAmount.Value;

            return maxApplicationAmount;
        }

        private int SaveCar(int clientId, ApplicationOnlineCar appOnlineCar)
        {
            var contractCar = new Car
            {
                ClientId = clientId,
                CollateralType = CollateralType.Car,
                Mark = appOnlineCar.Mark,
                Model = appOnlineCar.Model,
                ReleaseYear = appOnlineCar.ReleaseYear.Value,
                TransportNumber = appOnlineCar.TransportNumber,
                MotorNumber = appOnlineCar.MotorNumber,
                BodyNumber = appOnlineCar.BodyNumber,
                TechPassportNumber = appOnlineCar.TechPassportNumber,
                Color = appOnlineCar.Color,
                TechPassportDate = appOnlineCar.TechPassportDate.Value,
                ParkingStatusId = 1,
                VehicleMarkId = appOnlineCar.VehicleMarkId.Value,
                VehicleModelId = appOnlineCar.VehicleModelId.Value
            };

            _carService.Save(contractCar);

            return contractCar.Id;
        }

        private void SetContractChecks(Contract contract)
        {
            if (contract.Id != 0)
                contract.Checks = _contractCheckValueRepository.List(new ListQuery(), new { ContractId = contract.Id });

            if (contract.Checks.Any())
                return;

            var contractChecks = _contractCheckRepository.List(new ListQuery() { Page = null });

            contract.Checks.AddRange(contractChecks.Select(c => new ContractCheckValue()
            {
                AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                CreateDate = DateTime.Now,
                BeginDate = c.PeriodRequired ? contract.ContractDate : default,
                EndDate = c.PeriodRequired ? contract.ContractDate.AddYears(c.DefaultPeriodAddedInYears ?? 0) : default,
                Value = true,
                CheckId = c.Id,
                Check = c
            }).ToList());

            _contractService.Save(contract);
        }
    }
}
