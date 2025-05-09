using Pawnshop.Core;
using System;
using System.Collections.Generic;


namespace Pawnshop.Data.Models.Insurances
{
    public class InsuranceRevise : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Период сверки
        /// </summary>
        public string Period { get; set; }
        /// <summary>
        /// Идентификатор страховой компанией
        /// </summary>
        public int InsuranceCompanyId { get; set; }
        /// <summary>
        /// Название страховой компанией
        /// </summary>
        public string InsuranceCompanyName { get; set; }
        /// <summary>
        /// Статус
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int AutorId { get; set; }
        /// <summary>
        /// ФИО пользователя
        /// </summary>
        public string AutorName { get; set; }
        /// <summary>
        /// Количество полисов в ФинКоре
        /// </summary>
        public int TotalInsurancePoliciesFinCore { get; set; }
        /// <summary>
        /// Количество полисов в страховой компании
        /// </summary>
        public int TotalInsurancePoliciesInsuranceCompany { get; set; }
        /// <summary>
        /// Итоговая страховая сумма в ФинКоре
        /// </summary>
        public decimal TotalInsuranceAmountFinCore { get; set; }
        /// <summary>
        /// Итоговая страховая сумма в страховой компании
        /// </summary>
        public decimal TotalInsuranceAmountInsuranceCompany { get; set; }
        /// <summary>
        /// Итоговая страховая премия в ФинКоре
        /// </summary>
        public decimal TotalSurchargeAmountFinCore { get; set; }
        /// <summary>
        /// Итоговая страховая премия в страховой компании
        /// </summary>
        public decimal TotalSurchargeAmountInsuranceCompany { get; set; }
        /// <summary>
        /// Итоговые агентские вознаграждение в ФинКоре
        /// </summary>
        public decimal TotalAgencyFeesFinCore { get; set; }
        /// <summary>
        /// Итоговые агентские вознаграждение в страховой компании
        /// </summary>
        public decimal TotalAgencyFeesInsuranceCompany { get; set; }
        /// <summary>
        /// Количество полисов на возврат
        /// </summary>
        public int ReturnPolicies { get; set; }
        /// <summary>
        /// Итоговая страховая сумма в ФинКоре
        /// </summary>
        public decimal ReturnAgencyFees { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// Содержания сверки отчетов со страховой компанией
        /// </summary>
        public List<InsuranceReviseRow> Rows { get; set; }
        public int TotalInsurancePolicies { get { return TotalInsurancePoliciesFinCore - TotalInsurancePoliciesInsuranceCompany; } }
        public decimal TotalInsuranceAmount { get { return TotalInsuranceAmountFinCore - TotalInsuranceAmountInsuranceCompany; } }
        public decimal TotalSurchargeAmount { get { return TotalSurchargeAmountFinCore - TotalSurchargeAmountInsuranceCompany; } }
        public decimal TotalAgencyFees { get { return TotalAgencyFeesFinCore- TotalAgencyFeesInsuranceCompany; }}
    }
}
