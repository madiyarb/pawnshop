using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Pawnshop.Web.Models.NationalBankExchangeRates
{


    /// <summary>
    /// Дата обменных курсов
    /// </summary>
    [XmlRoot(ElementName = "item")]
    public class CurrencyItem
    {
        /// <summary>
        /// Полное наименование валюты
        /// </summary>
        [XmlElement(ElementName = "fullname")]
        [Display(Name = "Полное наименование валюты")]
        public string Fullname { get; set; }

        /// <summary>
        /// Сокращенное наименование валюты
        /// </summary>
        [XmlElement(ElementName = "title")]
        [Display(Name = "Сокращенное наименование валюты")]
        public string Title { get; set; }

        /// <summary>
        /// Обменный курс
        /// </summary>
        [XmlElement(ElementName = "description")]
        [Display(Name = "Обменный курс")]
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Количество единиц валюты за обменный курс
        /// </summary>
        [XmlElement(ElementName = "quant")]
        [Display(Name = "Количество единиц валюты за обменный курс")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Показатель изменения(выше/ниже/без изменения) по сравнению с предыдущим днем
        /// </summary>
        [XmlElement(ElementName = "index")]
        [Display(Name = "Показатель изменения(выше/ниже/без изменения) по сравнению с предыдущим днем")]
        public string Index { get; set; }

        /// <summary>
        /// Изменение курса по сравнению с предыдущим днем
        /// </summary>
        [XmlElement(ElementName = "change")]
        [Display(Name = "Изменение курса по сравнению с предыдущим днем")]
        public string Change { get; set; }
    }
}
