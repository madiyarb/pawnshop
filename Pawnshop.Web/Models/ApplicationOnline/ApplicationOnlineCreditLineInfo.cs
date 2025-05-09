using System;

namespace Pawnshop.Web.Models.ApplicationOnline
{
    public class ApplicationOnlineCreditLineInfo
    {
        /// <summary>
        /// Идентификатор КЛ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер договора КЛ
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Дата создания КЛ
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата завершения КЛ
        /// </summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Остаток ОД по КЛ
        /// </summary>
        public decimal DebtLeft { get; set; }

        /// <summary>
        /// Остаток доступного лимита
        /// </summary>
        public decimal CreditLineLimit { get; set; }

        /// <summary>
        /// Сумма выкупа
        /// </summary>
        public decimal CurrentDebt { get; set; }

        /// <summary>
        /// Общий лимит КЛ
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Макс текущая просрочка
        /// </summary>
        public int PaymentExpiredDays { get; set; }

        /// <summary>
        /// Статус стоянки
        /// </summary>
        public string ParkingStatus { get; set; }

        /// <summary>
        /// Статус ЧСИ
        /// </summary>
        public string InscriptionStatus { get; set; }

        /// <summary>
        /// Признак наличия ЧСИ
        /// </summary>
        public bool HasInscription { get; set; }
    }
}
