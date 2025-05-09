using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Localizations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.LoanPercent
{
    public class LoanPercentService : ILoanPercentService
    {
        private readonly FunctionSettingRepository _functionSettingRepository;
        private readonly LoanPercentRepository _loanPercentRepository;

        public LoanPercentService(
            FunctionSettingRepository functionSettingRepository,
            LoanPercentRepository loanPercentRepository)
        {
            _functionSettingRepository = functionSettingRepository;
            _loanPercentRepository = loanPercentRepository;
        }

        public LoanPercentSetting Get(int id)
        {
            try
            {
                return _loanPercentRepository.Get(id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<LoanPercentSetting>> GetChild(int id)
        {
            return await _loanPercentRepository.GetChild(id);
        }

        public async Task<List<LoanPercentFromMobile>> GetListFromMobile(string productTypeCode)
        {
            var productsRaw = await _loanPercentRepository.FindListByProductTypeCode(productTypeCode);

            var groupProductList = productsRaw
                .Where(x => x.IsActual)
                .GroupBy(x => new { x.ParentId, x.ScheduleType, x.IsFloatingDiscrete })
                .ToList();

            var firstTrancheMinSum = _functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__ONLINE_FIRST_TRANCHE_MIN_AMOUNT)?.MoneyValue ?? 0;

            var productListView = new List<LoanPercentFromMobile>();

            foreach (var products in groupProductList)
            {
                var product = products.OrderBy(x => x.IsInsuranceAvailable).FirstOrDefault();
                var minPercent = products.OrderBy(x => x.LoanPercent).First();
                var maxPercnet = products.OrderByDescending(x => x.LoanPercent).First();

                var insuranceType = 0;

                if (product.ContractClass == ContractClass.Credit)
                    insuranceType = product.IsInsuranceAvailable ? 2 : 0;

                else if (products.Any(x => x.IsInsuranceAvailable) && products.Any(x => !x.IsInsuranceAvailable))
                    insuranceType = 2;

                else if (products.Any(x => x.IsInsuranceAvailable))
                    insuranceType = 1;
                else
                    insuranceType = 0;

                productListView.Add(new LoanPercentFromMobile
                {
                    Title = product.Title?.Select(x =>
                        new LocalizationView
                        {
                            Language = x.Language,
                            Value = x.Value,
                        }).ToList(),
                    Description = product.Description?.Select(x =>
                        new LocalizationView
                        {
                            Language = x.Language,
                            Value = x.Value,
                        }).ToList(),
                    ProductId = product.Id,
                    Insurance = insuranceType,
                    Percent = Math.Round(minPercent.LoanPercent * ((int?)minPercent.ContractPeriodToType ?? 0), 2),
                    MinPercent = Math.Round(minPercent.LoanPercent * ((int?)minPercent.ContractPeriodToType ?? 0), 2),
                    MaxPercent = Math.Round(maxPercnet.LoanPercent * ((int?)maxPercnet.ContractPeriodToType ?? 0), 2),
                    MinSum = product.ContractClass == ContractClass.Tranche ? firstTrancheMinSum : product.LoanCostFrom,
                    MaxSum = product.LoanCostTo,
                    MinTrancheSum = product.ContractClass == ContractClass.Tranche ? product.LoanCostFrom : (int?)default,
                    MinMonth = (product.ContractPeriodFrom * (int?)product.ContractPeriodFromType) / (int)product.PaymentPeriodType ?? 0,
                    MaxMonth = (product.ContractPeriodTo * (int?)product.ContractPeriodToType) / (int)product.PaymentPeriodType ?? 0,
                    ScheduleType = (int)product.ScheduleType,
                });
            }

            return productListView;
        }

        public List<LoanPercentOnlineView> GetListForOnline(ContractClass contractClass)
        {
            var productsRaw = _loanPercentRepository.GetListForOnline(contractClass);

            var groupProductList = productsRaw
                .GroupBy(x => new { x.ParentId, x.ScheduleType, x.IsFloatingDiscrete, x.IsActual })
                .ToList();

            var firstTrancheMinSum = _functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__ONLINE_FIRST_TRANCHE_MIN_AMOUNT)?.MoneyValue ?? 0;

            var productsView = new List<LoanPercentOnlineView>();

            foreach (var products in groupProductList)
            {
                var product = products.OrderBy(x => x.IsInsuranceAvailable).FirstOrDefault();
                var minPercent = products.OrderBy(x => x.LoanPercent).First();
                var maxPercnet = products.OrderByDescending(x => x.LoanPercent).First();

                var insuranceType = 0;

                if (product.ContractClass == ContractClass.Credit)
                    insuranceType = product.IsInsuranceAvailable ? 2 : 0;

                else if (products.Any(x => x.IsInsuranceAvailable) && products.Any(x => !x.IsInsuranceAvailable))
                    insuranceType = 2;

                else if (products.Any(x => x.IsInsuranceAvailable))
                    insuranceType = 1;
                else
                    insuranceType = 0;

                productsView.Add(new LoanPercentOnlineView
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Insurance = insuranceType,
                    Percent = Math.Round(minPercent.LoanPercent * ((int?)minPercent.ContractPeriodToType ?? 0), 2),
                    MinPercent = Math.Round(minPercent.LoanPercent * ((int?)minPercent.ContractPeriodToType ?? 0), 2),
                    MaxPercent = Math.Round(maxPercnet.LoanPercent * ((int?)maxPercnet.ContractPeriodToType ?? 0), 2),
                    MinSum = product.ContractClass == ContractClass.Tranche ? firstTrancheMinSum : product.LoanCostFrom,
                    MaxSum = product.LoanCostTo,
                    MinTrancheSum = product.ContractClass == ContractClass.Tranche ? product.LoanCostFrom : (int?)default,
                    MinMonth = (product.ContractPeriodFrom * (int?)product.ContractPeriodFromType) / (int)product.PaymentPeriodType ?? 0,
                    MaxMonth = (product.ContractPeriodTo * (int?)product.ContractPeriodToType) / (int)product.PaymentPeriodType ?? 0,
                    ScheduleType = (int)product.ScheduleType,
                    isActual = product.IsActual,
                });
            }

            return productsView;
        }


        private decimal CalcPercentYear(decimal loanPercent, PeriodType? contractPeriodType)
        {
            if (contractPeriodType == null)
                return 0;

            return contractPeriodType.Value switch
            {
                PeriodType.Year => Math.Round(loanPercent * ((int?)contractPeriodType ?? 0), 2),
                PeriodType.HalfYear => Math.Round(loanPercent * ((int?)contractPeriodType ?? 0) * 2, 2),
                PeriodType.Month => Math.Round(loanPercent * ((int?)contractPeriodType ?? 0) * 12, 2),
                PeriodType.Week => Math.Round(loanPercent * ((int?)contractPeriodType ?? 0) / 7 * (int)PeriodType.Year, 2),
                PeriodType.Day => Math.Round(loanPercent * ((int?)contractPeriodType ?? 0) * (int)PeriodType.Year, 2),
                _ => 0
            };
        }
    }
}
