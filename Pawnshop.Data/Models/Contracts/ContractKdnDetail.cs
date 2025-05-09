using Pawnshop.Core;
using Pawnshop.Data.Models.Files;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractKdnDetail : IEntity
    {
        /// <summary>
        /// Идентификатор детализации
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Вид заемщика
        /// </summary>
        public int SubjectTypeId { get; set; }

        /// <summary>
        /// Сумма ежемесячных платежей
        /// </summary>
        public decimal MonthlyPaymentAmount { get; set; }

        /// <summary>
        /// Сумма просроченных платежей
        /// </summary>
        public decimal OverdueAmount { get; set; }

        /// <summary>
        /// Id документа
        /// </summary>
        public int? FileRowId { get; set; }

        /// <summary>
        /// Id документа
        /// </summary>
        public FileRow FileRow { get; set; }

        /// <summary>
        /// Идентификатор заявки
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime DeleteDate { get; set; }

        /// <summary>
        /// Наименование кредитора
        /// </summary>
        public string CreditorName { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Дата начала договора
        /// </summary>
        public DateTime ContractStartDate { get; set; }

        /// <summary>
        /// Дата окончания договора
        /// </summary>
        public DateTime ContractEndDate { get; set; }

        /// <summary>
        /// Общая сумма договора
        /// </summary>
        public decimal ContractTotalAmount { get; set; }

        /// <summary>
        /// Количество предстоящих платежей
        /// </summary>
        public int ForthcomingPaymentCount { get; set; }

        /// <summary>
        /// Не погашенная сумма долга
        /// </summary>
        public decimal OutstandingAmount { get; set; }

        /// <summary>
        /// Количество дней просрочки
        /// </summary>
        public int OverdueDaysCount { get; set; }

        /// <summary>
        /// Вид обеспечения
        /// </summary>
        public string CollateralType { get; set; }

        /// <summary>
        /// Стоимость обеспечения
        /// </summary>
        public decimal CollateralCost { get; set; }

        /// <summary>
        /// Кредит погашен
        /// </summary>
        public bool IsLoanPaid { get; set; }

        /// <summary>
        /// Кредитная линия
        /// </summary>
        public bool IsCreditCard { get; set; }

        /// <summary>
        /// Дата проставления отметки о том что кредит закрыт
        /// </summary>
        public DateTime? DateUpdated { get; set; }
        
        /// <summary>
        /// Пользователь поставивший отметку о том что кредит закрыт
        /// </summary>
        public int? UserUpdated { get; set; }

        /// <summary>
        /// Отметка о том что данный договор был создан при заявке на добор и требуется обновление ContractId
        /// Изначально ContractId записывается родительский. При самом добобе обновляем на ContractId дочернего
        /// </summary>
        public bool IsFromAdditionRequest { get; set; }

        /// <summary>
        /// Дата заявки на добор
        /// </summary>
        public DateTime? AdditionRequestDate { get; set; }

        /// <summary>
        /// Сумма платежа по данному кредиту, которая учитывается при расчете долговой нагрузки
        /// </summary>
        public decimal Amount4Kdn { get; set; }

        /// <summary>
        /// Сумма кредитного лимита
        /// </summary>
        public decimal CreditLimitAmount { get; set; }
    }
}
