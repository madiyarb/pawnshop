using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Base;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.PrintTemplates;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Localizations;

namespace Pawnshop.Data.Models.LoanSettings
{
    /// <summary>
    /// Настройка процентов кредита
    /// </summary>
    public class LoanPercentSetting : IEntity
    {

        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Организация
        /// </summary>
        [RequiredId(ErrorMessage = "Поле организация обязательно для заполнения")]
        public int OrganizationId { get; set; }

        /// <summary>
        /// Филиал, в котором создан договор
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// Филиал, в котором создан договор
        /// </summary>
        public Group Branch { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Тип карты
        /// </summary>
        public CardType? CardType { get; set; }

        /// <summary>
        /// Ссуда от
        /// </summary>
        public int LoanCostFrom { get; set; }

        /// <summary>
        /// Ссуда до
        /// </summary>
        public int LoanCostTo { get; set; }

        /// <summary>
        /// Срок залога (дней)
        /// </summary>
        public int LoanPeriod { get; set; }

        /// <summary>
        /// Мин срок залога (дней)
        /// </summary>
        public int MinLoanPeriod { get; set; }

        /// <summary>
        /// Процент кредита
        /// </summary>
        public decimal LoanPercent { get; set; }

        /// <summary>
        /// Актуален
        /// </summary>
        public bool IsActual { get; set; } = true;

        /// <summary>
        /// Является продуктом
        /// </summary>
        public bool IsProduct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AdditionAvailable { get; set; }

        /// <summary>
        /// Название продукта
        /// </summary>
        [CustomValidation(typeof(LoanPercentSetting), "NameValidate")]
        public string Name { get; set; }

        /// <summary>
        /// Название продукта на втором языке
        /// </summary>
        [CustomValidation(typeof(LoanPercentSetting), "NameValidate")]
        public string NameAlt { get; set; }

        /// <summary>
        /// Вид графика
        /// </summary>
        [CustomValidation(typeof(LoanPercentSetting), "ScheduleTypeValidate")]
        public ScheduleType? ScheduleType { get; set; }

        /// <summary>
        /// Срок кредитования от
        /// </summary>
        public int? ContractPeriodFrom { get; set; }
        /// <summary>
        /// Период срока кредитования от
        /// </summary>
        public PeriodType? ContractPeriodFromType { get; set; }

        /// <summary>
        /// Срок кредитования до
        /// </summary>
        public int? ContractPeriodTo { get; set; }
        /// <summary>
        /// Период срока кредитования до
        /// </summary>
        public PeriodType? ContractPeriodToType { get; set; }

        /// <summary>
        /// Срок погашения основного долга
        /// </summary>
        public int? DebtPeriod { get; set; }
        /// <summary>
        /// Период погашения основного долга
        /// </summary>
        public PeriodType? DebtPeriodType { get; set; }

        /// <summary>
        /// Срок погашения процентов
        /// </summary>
        public int? PaymentPeriod { get; set; }
        /// <summary>
        /// Период погашения процентов
        /// </summary>
        public PeriodType? PaymentPeriodType { get; set; }

        /// <summary>
        /// Первоначальный взнос
        /// </summary>
        [CustomValidation(typeof(LoanPercentSetting), "InitialFeeRequiredValidate")]
        public int? InitialFeeRequired { get; set; }

        [CustomValidation(typeof(LoanPercentSetting), "ProductTypeValidate")]
        public int? ProductTypeId { get; set; }
        public LoanProductType ProductType { get; set; }

        public int? PartialPaymentRequiredSum { get; set; }
        [CustomValidation(typeof(LoanPercentSetting), "PartialPaymentRequiredPercentValidate")]
        public int? PartialPaymentRequiredPercent { get; set; }

        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        [CustomValidation(typeof(LoanPercentSetting), "CurrencyValidate")]
        public int? CurrencyId { get; set; }
        public Currency Currency { get; set; }

        /// <summary>
        /// Идентификатор родителя
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// Родитель
        /// </summary>
        public LoanPercentSetting Parent { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Обязательные субъекты для заполнения
        /// </summary>
        public List<LoanRequiredSubject> RequiredSubjects { get; set; }
        /// <summary>
        /// Шаблоны для распечатки
        /// </summary>
        public List<PrintTemplate> PrintTemplates { get; set; }
        /// <summary>
        /// Огравничения
        /// </summary>
        public List<LoanPercentSettingRestriction> Restrictions { get; set; }

        public List<ContractPeriod> PossibleContractPeriods => CalculatePossibleContractPeriods();

        public List<ContractPeriod> PossibleDebtGracePeriods => CalculatePossibleDebtGracePeriods();

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Тип договора
        /// </summary>
        public int ContractTypeId { get; set; }

        /// <summary>
        /// Тип срочности договора
        /// </summary>
        public int PeriodTypeId { get; set; }
        /// <summary>
        /// Схема очередности погашения
        /// </summary>
        public int PaymentOrderSchema { get; set; }
        /// <summary>
        /// Дата начала доступности продукта
        /// </summary>
        public DateTime? AvailableDateFrom { get; set; }
        /// <summary>
        /// Дата окончания доступности договора
        /// </summary>
        public DateTime? AvailableDateTill { get; set; }
        /// <summary>
        /// Максимальное значение LTV
        /// </summary>
        public decimal? LTV { get; set; }

        public bool IsInsuranceAvailable { get; set; }
        public bool UsePenaltyLimit { get; set; }

        /// <summary>
        /// Минимальное значение для льготного периода
        /// </summary>
        public int? DebtGracePeriodFrom { get; set; }
        public PeriodType? DebtGracePeriodFromType { get; set; }

        /// <summary>
        /// Максимальное значение для льготного периода
        /// </summary>
        public int? DebtGracePeriodTo { get; set; }
        public PeriodType? DebtGracePeriodToType { get; set; }
        /// <summary>
        /// Плавающая ставка для дискретного кредита
        /// </summary>
        public bool IsFloatingDiscrete { get; set; }

        public List<LoanPercentSettingInsuranceCompany> InsuranceCompanies { get; set; }
        public List<LoanSettingRate> LoanSettingRates { get; set; }

        public List<LoanSettingProductTypeLTV>? ProductTypeLTVs { get; set; } = null;

        /// <summary>
        /// Необходимость расчета КДН для продукта
        /// </summary>
        public bool IsKdnRequired { get; set; }
        public ContractClass ContractClass { get; set; }


        public int PaymentCount(int loanPeriod)
        {
            return (int)Math.Floor((loanPeriod / ((decimal?)PaymentPeriod * (decimal?)PaymentPeriodType)) ?? 0);
        }

        /// <summary>
        /// Система использующая продукт
        /// </summary>
        public UseSystemType UseSystemType { get; set; }

        public int? TitleId { get; set; }
        public List<Localization> Title { get; set; }
        public int? DescriptionId { get; set; }
        public List<Localization> Description { get; set; }

        /// <summary>
        /// Ликвидность авто
        /// </summary>
        public bool IsLiquidityOn { get; set; }
        
        /// <summary>
        /// Добавочный лимит от стоимости авто для оплаты страховки
        /// </summary>
        public bool IsInsuranceAdditionalLimitOn { get; set; }

        private List<ContractPeriod> CalculatePossibleContractPeriods()
        {
            if (!ContractPeriodFrom.HasValue || !ContractPeriodTo.HasValue || !PaymentPeriod.HasValue) return null;
            var minValue = ContractPeriodFrom * (int)ContractPeriodFromType;
            var result = new List<ContractPeriod>();
            while (minValue <= (ContractPeriodTo * (int)ContractPeriodToType))
            {
                result.Add(new ContractPeriod
                {
                    Period = minValue.Value / (int)PaymentPeriodType,
                    PeriodType = PaymentPeriodType.Value
                });
                minValue += PaymentPeriod * (int)PaymentPeriodType;
            }

            return result;
        }

        private List<ContractPeriod> CalculatePossibleDebtGracePeriods()
        {
            if (!DebtGracePeriodFrom.HasValue || !DebtGracePeriodTo.HasValue || !DebtGracePeriodFromType.HasValue || !DebtGracePeriodToType.HasValue || !PaymentPeriod.HasValue) return null;
            var minValue = DebtGracePeriodFrom * (int)DebtGracePeriodFromType;
            var result = new List<ContractPeriod>();
            while (minValue <= (DebtGracePeriodTo * (int)DebtGracePeriodToType))
            {
                result.Add(new ContractPeriod
                {
                    Period = minValue.Value / (int)PaymentPeriodType,
                    PeriodType = PaymentPeriodType.Value
                });
                minValue += PaymentPeriod * (int)PaymentPeriodType;
            }
            return result;
        }

        public static ValidationResult ScheduleTypeValidate(ScheduleType? value, ValidationContext context)
        {
            var setting = (LoanPercentSetting)context.ObjectInstance;

            if (setting.IsProduct && !value.HasValue)
            {
                return new ValidationResult("Вид начисления(графика) обязателен к заполнению");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult NameValidate(string value, ValidationContext context)
        {
            var setting = (LoanPercentSetting)context.ObjectInstance;

            if (setting.IsProduct && string.IsNullOrEmpty(value))
            {
                return new ValidationResult("Название продукта на двух языках обязательно к заполнению");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult ProductTypeValidate(int? value, ValidationContext context)
        {
            var setting = (LoanPercentSetting)context.ObjectInstance;

            if (setting.IsProduct && !value.HasValue)
            {
                return new ValidationResult("Вид продукта обязателен к заполнению");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult InitialFeeRequiredValidate(int? value, ValidationContext context)
        {
            var setting = (LoanPercentSetting)context.ObjectInstance;

            if (setting.InitialFeeRequired.HasValue && (setting.InitialFeeRequired < 0 || setting.InitialFeeRequired > 100))
            {
                return new ValidationResult("Ошибка настройка первоначального взноса, должен быть между 0 и 100");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult PartialPaymentRequiredPercentValidate(int? value, ValidationContext context)
        {
            var setting = (LoanPercentSetting)context.ObjectInstance;

            if (setting.PartialPaymentRequiredPercent.HasValue && (setting.PartialPaymentRequiredPercent < 0 || setting.PartialPaymentRequiredPercent > 100))
            {
                return new ValidationResult("Ошибка настройки процента для ЧДП, должен быть между 0 и 100");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult CurrencyValidate(int? value, ValidationContext context)
        {
            var setting = (LoanPercentSetting)context.ObjectInstance;

            if (setting.IsProduct && !value.HasValue)
            {
                return new ValidationResult("Валюта обязательна к заполнению");
            }
            return ValidationResult.Success;
        }
    }
}