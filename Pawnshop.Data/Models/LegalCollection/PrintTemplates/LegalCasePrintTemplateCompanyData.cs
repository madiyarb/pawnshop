namespace Pawnshop.Data.Models.LegalCollection.PrintTemplates
{
    public class LegalCasePrintTemplateCompanyData
    {
        /// <summary>
        /// Наименование компаний
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// БИН компаний
        /// </summary>
        public string BIN { get; set; }
        /// <summary>
        /// ИИК компаний
        /// </summary>
        public string IIK { get; set; }
        /// <summary>
        /// БИК компаний
        /// </summary>
        public string BIK { get; set; }
        /// <summary>
        /// Наименование банка
        /// </summary>
        public string BankFullName { get; set; }
        /// <summary>
        /// Адресс компаний
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Фактический адресс компаний
        /// </summary>
        public string ActualAddress { get; set; }
    }
}
