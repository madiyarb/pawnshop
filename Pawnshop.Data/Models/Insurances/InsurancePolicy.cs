using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Models.Insurances
{
	/// <summary>
	/// Полисы страхования
	/// </summary>
	public class InsurancePolicy : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int? ContractId { get; set; }
        public Contract Contract { get; set; }
        public int RootContractId { get; set; }
		/// <summary>
		/// Начало действия полиса
		/// </summary>
		public DateTime StartDate { get; set; }
		/// <summary>
		/// Окончание действия полиса
		/// </summary>
		public DateTime EndDate { get; set; }
		/// <summary>
		/// Страховая сумма
		/// </summary>
		public decimal InsuranceAmount { get; set; }
		/// <summary>
		/// Страховая премия
		/// </summary>
		public decimal InsurancePremium { get; set; }
		/// <summary>
		/// Номер полиса
		/// </summary>
		public string PoliceNumber { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
		/// <summary>
		/// Идентификатор запроса в СК
		/// </summary>
		public int PoliceRequestId { get; set; }
        public InsurancePoliceRequest PoliceRequest { get; set; }
		public decimal SurchargeAmount { get; set; }
		public decimal YearPremium { get; set; }

		/// <summary>
		/// Версия алгоритма расчета страховки
		/// </summary>
		public int AlgorithmVersion { get; set; }

		/// <summary>
		/// Страховая премия, рассчитанная при текущем доборе, минус страховая премия предыдущего полиса
		/// </summary>
		public int EsbdAmount { get; set; }
	}
}