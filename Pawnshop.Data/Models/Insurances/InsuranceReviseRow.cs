using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Insurances
{
    public class InsuranceReviseRow : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор сверки отчетов со страховой компанией
        /// </summary>
        public int InsuranceReviseId { get; set; }
        /// <summary>
        /// Идентификатор полиса
        /// </summary>
        public int? InsurancePolicyId { get; set; }
        /// <summary>
        /// Номер полиса
        /// </summary>
        public string InsurancePolicyNumber { get; set; }
        /// <summary>
        /// Дата начала действия полиса
        /// </summary>
        public DateTime? InsuranceStartDate { get; set; }
        /// <summary>
        /// Дата окончания действия полиса
        /// </summary>
        public DateTime? InsuranceEndDate { get; set; }
        /// <summary>
        /// Страховая премия
        /// </summary>
        public decimal SurchargeAmount { get; set; }
        /// <summary>
        /// Агентское вознаграждение
        /// </summary>
        public decimal AgencyFees { get; set; }
        /// <summary>
        /// Страховая сумма
        /// </summary>
        public decimal InsuranceAmount { get; set; }
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int? ClientId { get; set; }
        /// <summary>
        /// ФИО клиента
        /// </summary>
        public string ClientFullName { get; set; }
        /// <summary>
        /// ИИН клиента
        /// </summary>
        public string ClientIdentityNumber { get; set; }
        /// <summary>
        /// Идентификатор филиал
        /// </summary>
        public int? BranchId { get; set; }
        /// <summary>
        /// Название филиал
        /// </summary>
        public string BranchName { get; set; }
        /// <summary>
        /// Статус
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Примечания
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}
