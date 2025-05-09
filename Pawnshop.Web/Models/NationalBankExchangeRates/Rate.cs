using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Pawnshop.Web.Models.NationalBankExchangeRates
{
    /// <summary>
    /// Обменные курсы нацбанка
    /// </summary>
    [XmlRoot(ElementName = "rates")]
    public class Rate
    {

        [XmlElement(ElementName = "generator")]
        public string Generator { get; set; }

        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "link")]
        public string Link { get; set; }

        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        [XmlElement(ElementName = "copyright")]
        public string Copyright { get; set; }

        /// <summary>
        /// Дата обменных курсов
        /// </summary>
        [XmlElement(ElementName = "date")]
        public string DateString { get; set; }

        [Display(Name = "Дата")]
        public DateTime Date => Convert.ToDateTime(DateString);

        /// <summary>
        /// Список валют обменных курсов
        /// </summary>
        [XmlElement(ElementName = "item")]
        public List<CurrencyItem> Currencies { get; set; }
    }
}
