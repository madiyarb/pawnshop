using Itenso.TimePeriod;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Insurance;
using System.Collections.Generic;
using System.Linq;
using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;
using Newtonsoft.Json;

namespace Pawnshop.Services.Insurance
{
    public class InsurancePremiumCalculator : IInsurancePremiumCalculator
    {
        private readonly IContractService _contractService;
        private readonly LoanPercentSettingInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly InsurancePolicyRepository _insurancePolicyRepository;
        private readonly InsuranceRateRepository _insuranceRateRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly IEventLog _eventLog;

        public InsurancePremiumCalculator(
            PayTypeRepository payTypeRepository,
            InsurancePolicyRepository insurancePolicyRepository,
            LoanPercentSettingInsuranceCompanyRepository insuranceCompanyRepository,
            InsuranceRateRepository insuranceRateRepository,
            LoanPercentRepository loanPercentRepository,
            IContractService contractService,
            IEventLog eventLog)
        {
            _payTypeRepository = payTypeRepository;
            _insurancePolicyRepository = insurancePolicyRepository;
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _insuranceRateRepository = insuranceRateRepository;
            _loanPercentRepository = loanPercentRepository;
            _contractService = contractService;
            _eventLog = eventLog;
        }

        public void CheckInsurancePremium(decimal cost, InsurancePolicy policy, int settingId)
        {
            var insurancePremium = GetInsuranceDataV2(cost - policy.InsurancePremium, policy.PoliceRequest.InsuranceCompanyId, settingId).InsurancePremium;

            if (insurancePremium != policy.InsurancePremium)
                throw new PawnshopApplicationException("Премия на договоре не совпадает с премией на страховом полисе");
        }

        public decimal GetAdditionInsuranceAmount(int contractId, decimal cost, DateTime? date, int? closedParentId = null)
        {
            PayType defaultPayType = _payTypeRepository.Find(new { IsDefault = true });

            decimal insuranceAmount = 0;

            var newContractDebtCost = _contractService.GetAccountBalance(contractId, date.HasValue ? date.Value : DateTime.Now);
            newContractDebtCost += _contractService.GetOverdueAccountBalance(contractId, date.HasValue ? date.Value : DateTime.Now);

            var insurancePolicies = _insurancePolicyRepository.List(new ListQuery(), new { RootContractId = closedParentId ?? contractId });

            bool previousPolicyOldAlgorithm = false;

            if (insurancePolicies.Count() > 1 && (insurancePolicies.OrderByDescending(x => x.Id).First().AlgorithmVersion == 1 ||
                                                  insurancePolicies.OrderByDescending(x => x.Id).First().AlgorithmVersion == 3))
                previousPolicyOldAlgorithm = true;

            decimal insurancePolicyCost = 0;

            if (insurancePolicies != null && insurancePolicies.Any())
            {
                var actualInsurancePolicies = insurancePolicies.Where(t => t.EndDate.Date >= DateTime.Now.Date && t.ContractId.HasValue);

                if (previousPolicyOldAlgorithm)
                    insurancePolicyCost = actualInsurancePolicies.Sum(t => t.InsurancePremium);
                else
                    if (actualInsurancePolicies.Count() > 0)
                    insurancePolicyCost = actualInsurancePolicies.OrderByDescending(x => x.Id).FirstOrDefault().InsuranceAmount;
            }

            var debtLeft = newContractDebtCost - insurancePolicyCost;
            insuranceAmount = debtLeft + cost;

            return insuranceAmount;
        }

        public InsuranceRequestData GetInsuranceData(decimal cost, int insuranceCompanyId, int settingId)
        {
            var loanPercentSetting = _loanPercentRepository.Get(settingId);

            if (!loanPercentSetting.IsInsuranceAvailable)
                return new InsuranceRequestData()
                {
                    InsurancePremium = 0,
                    LoanCost = cost
                };

            var insuranceRates = ConvertInsuranceRates(_insuranceRateRepository.List(new ListQuery(), new { InsuranceCompanyId = insuranceCompanyId }));

            if (insuranceRates is null || insuranceRates.Count() == 0)
                throw new PawnshopApplicationException("Для выбранной страховой компании не найдены тарифы");

            var maxRateAmount = insuranceRates.Max(t => t.InsuranceRate.AmountTo);

            var loanCost = cost >= maxRateAmount ? maxRateAmount - 1 : cost;

            var insuranceRate = insuranceRates.FirstOrDefault(t => loanCost >= t.InsuranceRate.AmountFrom && loanCost < t.InsuranceRate.AmountTo);

            if (insuranceRate is null)
            {
                insuranceRate = insuranceRates.FirstOrDefault(t =>
                    loanCost > t.PreviousAmountFrom && loanCost < t.InsuranceRate.AmountFrom);

                if (insuranceRate != null && insuranceRate.PreviousAmountFrom != 0)
                    loanCost = insuranceRate.InsuranceRate.AmountFrom;
                else
                    return new InsuranceRequestData();
            }

            var rate = insuranceRate.InsuranceRate.Rate / 100;
            var insuranceCompanySettings = GetLoanPercentSettingInsuranceCompany(insuranceCompanyId, settingId);

            var maxPremium = insuranceCompanySettings.MaxPremium;
            var insurancePremium = Math.Round(loanCost / (1 - rate) * rate, MidpointRounding.AwayFromZero);
            var insuranceAmount = Math.Ceiling(loanCost) + insurancePremium;

            var requestData = new InsuranceRequestData()
            {
                InsuranceEndDate = DateTime.Now.AddMonths(insuranceCompanySettings.InsurancePeriod),
                InsurancePeriod = insuranceCompanySettings.InsurancePeriod,
                InsuranceAmount = insuranceAmount,
                InsuranceRate = insuranceRate.InsuranceRate.Rate,
                InsurancePremium = insurancePremium,
                LoanCost = insuranceAmount
            };

            var isMaxAmount = maxPremium > 0 && insurancePremium >= maxPremium;

            if (isMaxAmount)
            {
                requestData.InsurancePremium = maxPremium;
                requestData.InsuranceAmount += 1;
                requestData.LoanCost = cost + maxPremium;
            }


            return requestData;
        }

        public InsuranceRequestData GetInsuranceDataV2(decimal cost, int insuranceCompanyId, int settingId, DateTime? additionDate = null, int? contractId = null, int? closedParentId = null)
        {
            var loanPercentSetting = _loanPercentRepository.Get(settingId);

            var model = new InsurancePremiumModel();
            model.LoanCost = cost;
            model.InsuranceCompanyId = insuranceCompanyId;
            model.SettingId = settingId;
            model.loanPercentSetting = loanPercentSetting;


            if (cost <= 0)
                return new InsuranceRequestData();

            if (!loanPercentSetting.IsInsuranceAvailable)
                return new InsuranceRequestData()
                {
                    InsurancePremium = 0,
                    LoanCost = cost
                };

            var insuranceRateList = _insuranceRateRepository.List(new ListQuery(), new { InsuranceCompanyId = insuranceCompanyId });
            var originMaxAmountTo = GetMaxAmountToFromInsuranceRates(insuranceRateList);
            var insuranceRates = ConvertInsuranceRates(insuranceRateList);
            var maxAmountTo = GetMaxAmountToFromInsuranceRates(insuranceRateList);

            if (insuranceRates is null || insuranceRates.Count() == 0)
                throw new PawnshopApplicationException("Для выбранной страховой компании не найдены тарифы");

            var maxRateAmount = insuranceRates.Max(t => t.InsuranceRate.AmountTo);

            //---------------------------------------------------------------------------------------------------------------------------------------
            bool hasPerviousePolicy = false;
            bool previousPolicyOldAlgorithm = false;
            bool isBorderAmount = false;

            decimal amountToAddIfBorder = 0;
            decimal insurancePolicyCost = 0;
            decimal prevInsurancePremium = 0;
            decimal prevInsuranceAmount = 0;
            decimal prevInsurancePremiumSum = 0;
            decimal prevInsuranceAmountSum = 0;
            decimal prevYearPremium = 0;
            decimal loanCost = 0;
            string prevInsurancePoliceNumber = string.Empty;
            string lastInsurancePoliceNumber = string.Empty;
            string allInsurancePoliceNumbers = string.Empty;
            DateTime prevInsuranceStartDate = default;
            DateTime firstInsuranceStartDate = default;
            DateTime prevInsuranceEndDate = default;
            DateTime addDate = DateTime.Today;
            if (additionDate.HasValue)
            {
                addDate = additionDate.Value;
                var insurancePolicies = _insurancePolicyRepository.List(new ListQuery(), new { RootContractId = closedParentId ?? contractId });

                if (insurancePolicies != null && insurancePolicies.Any())
                {
                    allInsurancePoliceNumbers = String.Join(",", insurancePolicies.Select(x => x.PoliceNumber));
                    var actualInsurancePolicies = insurancePolicies.Where(t => t.DeleteDate is null && t.EndDate.Date >= DateTime.Now.Date && t.ContractId.HasValue).OrderByDescending(x => x.Id);

                    var lastInsurancePolicy = actualInsurancePolicies.FirstOrDefault();
                    if (lastInsurancePolicy != null)
                    {
                        insurancePolicyCost = lastInsurancePolicy.InsuranceAmount - lastInsurancePolicy.InsurancePremium;//сумма полученная клиентом на руки

                        var firstActualPolicy = actualInsurancePolicies.OrderBy(t => t.Id).FirstOrDefault();//для заполения partnerID
                        var lastActualPolicy = actualInsurancePolicies.OrderByDescending(t => t.Id).FirstOrDefault();//для заполнения secondPartnerID
                        if (lastActualPolicy != null)
                        {
                            prevInsurancePoliceNumber = firstActualPolicy.PoliceNumber;//для заполения partnerID
                            lastInsurancePoliceNumber = lastActualPolicy.PoliceNumber;//для заполнения secondPartnerID
                            prevInsurancePremium = lastActualPolicy.InsurancePremium;//используется для расчета страховой премий для текущего Добора
                            prevInsuranceAmount = lastActualPolicy.InsuranceAmount;
                            prevYearPremium = lastActualPolicy.YearPremium;//используется для расчета surchargeAmount
                            prevInsuranceStartDate = lastActualPolicy.StartDate;//используется для расчета surchargeAmount
                            prevInsuranceEndDate = lastActualPolicy.EndDate;//используется для расчета surchargeAmount
                            hasPerviousePolicy = true;
                            firstInsuranceStartDate = firstActualPolicy.StartDate;
                        }
                        previousPolicyOldAlgorithm = lastInsurancePolicy.AlgorithmVersion == 1 || lastInsurancePolicy.AlgorithmVersion == 3;
                        prevInsurancePremiumSum = actualInsurancePolicies.Sum(x => x.InsurancePremium);
                        prevInsuranceAmountSum = actualInsurancePolicies.Sum(x => x.InsuranceAmount);
                    }
                }
            }
            //---------------------------------------------------------------------------------------------------------------------------------------
            bool isMaxRateAmount = false;

            if (previousPolicyOldAlgorithm)
            {
                isMaxRateAmount = cost >= maxRateAmount;
                loanCost = isMaxRateAmount ? maxRateAmount - 1 : cost;
            }
            else
            {
                isMaxRateAmount = cost + insurancePolicyCost >= maxRateAmount;//cost сумма выдачи или сумма Добора
                loanCost = isMaxRateAmount ? maxRateAmount - 1 : cost + insurancePolicyCost;//сумма которую получит клиент с учетом текущего Добора и суммы которую он ранее получил
            }

            //Если клиент уже получил маскимальную сумму и оплатил максимальную ставку по полису
            if (insurancePolicyCost > maxAmountTo)
                return new InsuranceRequestData();

            //Если до договору ранее уже был полис и он завершен, тогда при следующих Доборах мы не требуем новый полис
            if (hasPerviousePolicy && prevInsuranceEndDate < addDate)
                return new InsuranceRequestData();

            //loanCost 
            var insuranceRate = insuranceRates.FirstOrDefault(t => loanCost >= t.InsuranceRate.AmountFrom && loanCost < t.InsuranceRate.AmountTo);

            if (insuranceRate is null)
            {
                insuranceRate = insuranceRates.FirstOrDefault(t =>
                    loanCost > t.PreviousAmountFrom && loanCost < t.InsuranceRate.AmountFrom);

                if (insuranceRate != null && insuranceRate.PreviousAmountFrom != 0)
                {
                    amountToAddIfBorder = insuranceRate.InsuranceRate.AmountFrom - loanCost;
                    loanCost = insuranceRate.InsuranceRate.AmountFrom;
                }
                else
                {
                    var emptyData = new InsuranceRequestData();
                    _eventLog.Log(EventCode.InsurancePremiumCalculation, EventStatus.Success, EntityType.InsurancePremium, contractId, JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(emptyData));
                    return emptyData;
                }
            }

            var rate = insuranceRate.InsuranceRate.Rate / 100;
            var insuranceCompanySettings = GetLoanPercentSettingInsuranceCompany(insuranceCompanyId, settingId);

            var premiumAccuracy = insuranceCompanySettings.PremiumAccuracy;//если сумма доплаты менше чем PremiumAccuracy, тогда страховку не требовать
            var maxPremium = insuranceCompanySettings.MaxPremium;//90000
            var insurancePremium = Math.Round(loanCost / (1 - rate) * rate, MidpointRounding.AwayFromZero);

            decimal surchargeAmount = 0;
            decimal yearInsurancePremium = 0;
            decimal addLoanCost = 0;
            decimal insuranceAmount = 0;
            int diffPolicyEndAndAddDate = 0;
            int daysInYear = 0;
            string descrOfInsuranceText = "";
            decimal esbdAmount = 0;

            insuranceAmount = Math.Ceiling(loanCost) + insurancePremium;

            esbdAmount = insuranceAmount - prevInsuranceAmount;
            if (isMaxRateAmount) esbdAmount = originMaxAmountTo - prevInsuranceAmount;

            var requestData = new InsuranceRequestData();

            //для Добора
            if (hasPerviousePolicy && !previousPolicyOldAlgorithm)
            {
                DateDiff dateDiff = new DateDiff(addDate.Date, prevInsuranceEndDate.Date);
                diffPolicyEndAndAddDate = dateDiff.Days + 1;

                dateDiff = new DateDiff(firstInsuranceStartDate.Date, prevInsuranceEndDate.Date);
                daysInYear = dateDiff.Days + 1;

                //Сумма доплаты страховой премии
                surchargeAmount = Math.Round(((insurancePremium - prevYearPremium) / daysInYear) * diffPolicyEndAndAddDate, MidpointRounding.AwayFromZero);

                yearInsurancePremium = insurancePremium;//сохраняем годовую страховую премеию, чтобы ее сохранить в таблице InsurancePremium для след доборов
                insurancePremium = prevInsurancePremium + surchargeAmount;//предыдущая сумма полиса + сумма доплаты для Добора
                addLoanCost = Math.Ceiling(cost) + surchargeAmount;

                insuranceAmount = Math.Ceiling(loanCost) + insurancePremium;

                loanCost = Math.Ceiling(loanCost);

                descrOfInsuranceText = $"Требуется доплата в размере {string.Format("{0:N}", surchargeAmount)} тг. для переоформления страхового полиса на большую сумму пропорционально сроку пользования страховым полисом до даты его окончания.\n" +
                                       $"Сумма доплаты рассчитана по формуле: (({string.Format("{0:N}", yearInsurancePremium)}-{string.Format("{0:N}", prevYearPremium)}) /{daysInYear})*{diffPolicyEndAndAddDate}, где\n" +
                                       $"{string.Format("{0:N}", yearInsurancePremium)} - годовая страховая премия переоформляемого при данном доборе полиса по ставке {rate} от суммы {string.Format("{0:N}", loanCost)} тг.\n" +
                                       $"{string.Format("{0:N}", prevYearPremium)} - годовая страховая премия действующего полиса (до данного добора)\n" +
                                       $"{daysInYear} - срок действия страхового полиса в днях (1 год)\n" +
                                       $"{diffPolicyEndAndAddDate} - срок от даты изменения полиса (текущего добора) до даты окончания полиса в днях \n" +
                                       $"Версия алгоритма - {2}";

                if (!isMaxRateAmount) esbdAmount = insuranceAmount - prevInsuranceAmount;

            }
            else { esbdAmount = 0; } //для выдачи обнуляем
            if (hasPerviousePolicy && previousPolicyOldAlgorithm)
            {
                DateDiff dateDiff = new DateDiff(addDate.Date, prevInsuranceEndDate.Date);
                diffPolicyEndAndAddDate = dateDiff.Days + 1;

                dateDiff = new DateDiff(prevInsuranceStartDate.Date, prevInsuranceEndDate.Date);
                daysInYear = dateDiff.Days + 1;

                surchargeAmount = insurancePremium - prevInsurancePremiumSum;

                yearInsurancePremium = insurancePremium;
                insurancePremium = surchargeAmount;
                addLoanCost = Math.Ceiling(cost) + surchargeAmount;
                insuranceAmount = Math.Ceiling(cost) + yearInsurancePremium - prevInsuranceAmountSum;

                insuranceRate.InsuranceRate.Rate = 0;

                descrOfInsuranceText = $"Требуется доплата в размере {string.Format("{0:N}", surchargeAmount)} тг. для переоформления страхового полиса на большую сумму пропорционально сроку пользования страховым полисом до даты его окончания.\n" +
                                       $"Сумма доплаты рассчитана по формуле: {string.Format("{0:N}", yearInsurancePremium)}-{string.Format("{0:N}", prevInsurancePremiumSum)}, где\n" +
                                       $"{string.Format("{0:N}", yearInsurancePremium)} - годовая страховая премия переоформляемого при данном доборе полиса от суммы {string.Format("{0:N}", loanCost)} тг. (основной долг – сумма страховых премий + сумма добора). \n" +
                                       $"{string.Format("{0:N}", prevInsurancePremiumSum)} - общая страховая премия по действующим полисам\n" +
                                       $"Срок окончания полиса – {prevInsuranceEndDate.ToString("dd.MM.yyyy")}\n" +
                                       $"Версия алгоритма - {3}";

                requestData = new InsuranceRequestData()
                {
                    InsuranceEndDate = prevInsuranceEndDate,
                    InsurancePeriod = insuranceCompanySettings.InsurancePeriod,
                    InsuranceAmount = insuranceAmount,
                    InsuranceRate = insuranceRate.InsuranceRate.Rate,
                    InsurancePremium = insurancePremium,
                    LoanCost = addLoanCost,
                    SurchargeAmount = surchargeAmount,
                    YearPremium = yearInsurancePremium,
                    DescrOfInsuranceCalc = descrOfInsuranceText,
                    Eds = loanCost,
                    PrevPolicyNumber = "TAS01",//для заполения partnerID
                    LastPolicyNumber = allInsurancePoliceNumbers,//для заполнения secondPartnerID
                    AlgorithmVersion = 3,
                    AmountToAddIfBorder = amountToAddIfBorder
                };

                if (maxPremium > 0 && isMaxRateAmount)
                {
                    requestData.InsurancePremium = requestData.InsurancePremium;
                    requestData.InsuranceAmount = originMaxAmountTo - 1 - prevInsuranceAmountSum;
                    requestData.LoanCost = requestData.LoanCost;
                }

                if (surchargeAmount <= premiumAccuracy)
                {
                    var emptyData = new InsuranceRequestData();
                    _eventLog.Log(EventCode.InsurancePremiumCalculation, EventStatus.Success, EntityType.InsurancePremium, contractId, JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(emptyData));
                    return emptyData;
                }

                _eventLog.Log(EventCode.InsurancePremiumCalculation, EventStatus.Success, EntityType.InsurancePremium, contractId, JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(requestData));
                return requestData;
            }

            //если сумма доплаты равно 0 или меньше нуля или меньше суммы premiumAccuracy, тогда страховка не нужна
            if (hasPerviousePolicy && (surchargeAmount <= 0 || surchargeAmount <= premiumAccuracy))
                return new InsuranceRequestData();

            requestData = new InsuranceRequestData()
            {
                InsuranceEndDate = hasPerviousePolicy ? prevInsuranceEndDate : DateTime.Now.AddMonths(insuranceCompanySettings.InsurancePeriod).AddDays(-1),
                InsurancePeriod = insuranceCompanySettings.InsurancePeriod,
                InsuranceAmount = insuranceAmount,
                InsuranceRate = insuranceRate.InsuranceRate.Rate,
                InsurancePremium = insurancePremium,
                LoanCost = !hasPerviousePolicy ? insuranceAmount : addLoanCost,
                SurchargeAmount = !hasPerviousePolicy ? insurancePremium : surchargeAmount,
                YearPremium = !hasPerviousePolicy ? insurancePremium : yearInsurancePremium,
                DescrOfInsuranceCalc = !hasPerviousePolicy ? string.Empty : descrOfInsuranceText,
                Eds = loanCost,
                PrevPolicyNumber = prevInsurancePoliceNumber,//для заполения partnerID
                LastPolicyNumber = lastInsurancePoliceNumber,//для заполнения secondPartnerID
                AlgorithmVersion = 2,
                AmountToAddIfBorder = amountToAddIfBorder,
                Premium2 = Convert.ToInt32(Math.Ceiling(surchargeAmount)),
                EsbdAmount = Convert.ToInt32(Math.Ceiling(esbdAmount))
            };

            var isMaxAmount = maxPremium > 0 && isMaxRateAmount;

            if (isMaxAmount)
            {
                requestData.InsurancePremium = !hasPerviousePolicy ? maxPremium : requestData.InsurancePremium;
                requestData.InsuranceAmount = originMaxAmountTo;
                requestData.LoanCost = !hasPerviousePolicy ? cost + maxPremium : requestData.LoanCost;
            }

            _eventLog.Log(EventCode.InsurancePremiumCalculation, EventStatus.Success, EntityType.InsurancePremium, contractId, JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(requestData));
            return requestData;
        }

        public decimal GetLoanCostWithoutInsurancePremium(decimal loanCost)
        {
            var insuranceRates = _insuranceRateRepository.List(new ListQuery() { Page = null });

            var minValue = insuranceRates.OrderBy(x => x.AmountFrom).FirstOrDefault();
            var maxValue = insuranceRates.OrderByDescending(x => x.AmountTo).FirstOrDefault();

            if (minValue.AmountFrom > loanCost)
                return loanCost;

            if (loanCost >= maxValue.AmountTo)
                return loanCost - ((maxValue.AmountTo / 100) * maxValue.Rate);

            var findValue = insuranceRates.FirstOrDefault(x => loanCost >= x.AmountFrom && loanCost < x.AmountTo);
            var insuranceAmount = Math.Round(loanCost / 100 * findValue.Rate, 0);

            return loanCost - insuranceAmount;
        }

        public LoanPercentSettingInsuranceCompany GetLoanPercentSettingInsuranceCompany(int insuranceCompanyId, int settingId)
        {
            var loanPercentSettingInsuranceCompany = _insuranceCompanyRepository.Find(new { InsuranceCompanyId = insuranceCompanyId, SettingId = settingId });

            if (loanPercentSettingInsuranceCompany is null)
                throw new PawnshopApplicationException("Страховая компания не найдена");

            return loanPercentSettingInsuranceCompany;
        }

        public bool LoanCostCanUseInsurance(decimal loanCost)
        {
            var insuranceRateList = _insuranceRateRepository.List(new ListQuery() { Page = null });
            var insuranceRates = ConvertInsuranceRates(insuranceRateList);

            var minRate = insuranceRates.OrderBy(x => x.InsuranceRate.AmountFrom).FirstOrDefault();

            if (minRate.InsuranceRate.AmountFrom > loanCost)
                return false;

            return true;
        }


        private List<IndexRateModel> ConvertInsuranceRates(List<InsuranceRate> insuranceRates)
        {
            var list = new List<IndexRateModel>();

            foreach (var insuranceRate in insuranceRates)
            {
                var index = insuranceRates.IndexOf(insuranceRate) - 1;
                var percent = 1 - insuranceRate.Rate / 100;

                insuranceRate.AmountFrom *= percent;
                insuranceRate.AmountTo *= percent;

                list.Add(new IndexRateModel()
                {
                    InsuranceRate = insuranceRate,
                    PreviousAmountFrom = index != -1 ? insuranceRates[index].AmountFrom : 0
                });
            }

            return list;
        }

        private decimal GetMaxAmountToFromInsuranceRates(List<InsuranceRate> insuranceRates)
        {
            return insuranceRates.OrderByDescending(x => x.AmountTo).FirstOrDefault().AmountTo;
        }
    }
}