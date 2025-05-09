using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Pawnshop.Web.Models.Processing
{
    /// <summary>
    /// Внешняя информация для Qiwi
    /// </summary>
    [XmlRoot("response")]
    public class QiwiOuterModel
    {
        /// <summary>
        /// Номер транзакции в системе Qiwi
        /// </summary>
        [XmlElement("osmp_txn_id")]
        public Int64 Osmp_txn_id { get; set; }

        /// <summary>
        /// Код результата 
        /// </summary>
        [XmlElement("result")]
        public int Result { get; set; }

        /// <summary>
        /// Сведения по договору
        /// </summary>
        [XmlElement("orders")]
        public List<Orders> Orders { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        [XmlElement("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Расширенные сведения по договору
        /// </summary>
        [XmlElement("fields")]
        public Fields Fields { get; set; }

        /// <summary>
        /// Номер транзакции в системе (ActionId)
        /// </summary>
        [XmlElement("prv_txn")]
        public int Prv_txn { get; set; }

        /// <summary>
        /// Оплаченная сумма
        /// </summary>
        [XmlElement("sum")]
        public decimal Sum { get; set; }
    }

    public class Orders
    {
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        [XmlElement("orderId")]
        public int OrderId { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        [XmlElement("text")]
        public string Text { get; set; }

        /// <summary>
        /// Сумма к оплате
        /// </summary>
        [XmlElement("amount")]
        public decimal Amount { get; set; }
    }

    public class Fields
    {
        /// <summary>
        /// Номер договора
        /// </summary>
        [XmlElement("field1")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// ФИО клиента
        /// </summary>
        [XmlElement("field2")]
        public string ClientFullName { get; set; }

        /// <summary>
        /// Сумма к оплате
        /// </summary>
        [XmlElement("field3")]
        public decimal AmountForPay { get; set; }

        /// <summary>
        /// Дата начала договора
        /// </summary>
        [XmlElement("field4")]
        public string ContractDate { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        [XmlElement("field5")]
        public int ContractId { get; set; }

        /// <summary>
        /// Информация о позиции 
        /// </summary>
        [XmlElement("field6")]
        public string PositionInfo { get; set; }
    }
}
