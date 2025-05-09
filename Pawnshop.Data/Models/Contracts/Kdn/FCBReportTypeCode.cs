namespace Pawnshop.Data.Models.Contracts.Kdn
{
    public enum FCBReportTypeCode : short
    {
        /// <summary>
        /// Кредитный отчет – первичный юридическое лицо
        /// </summary>
        //CompanyInitial = 1,

        /// <summary>
        /// Кредитный отчет – первичный физическое лицо
        /// </summary>
        //IndividualInitial = 2,

        /// <summary>
        /// Кредитный отчет – стандартный юридическое лицо
        /// </summary>
        //CompanyStandard = 3,

        /// <summary>
        /// Кредитный отчет – стандартный физическое лицо
        /// </summary>
        IndividualStandard = 4,

        /// <summary>
        /// Кредитный отчет – расширенный юридическое лицо
        /// </summary>
        //CompanyAdvanced = 5,

        /// <summary>
        /// Кредитный отчет – расширенный физическое лицо
        /// </summary>
        IndividualAdvanced = 6,

        /// <summary>
        /// Идентификационный отчет – физическое лицо
        /// </summary>
        //IndividualIdentification = 7,

        /// <summary>
        /// Отчет, содержащий негативную информацию юридическое лицо
        /// </summary>
        //CompanyNegative = 14,

        /// <summary>
        /// Отчет, содержащий негативную информацию физическое лицо
        /// </summary>
        IndividualNegative = 13,

        /// <summary>
        /// Идентификационный отчет - юридическое лицо
        /// </summary>
        //CompanyIdentification = 20,

        /// <summary>
        /// Кредитный отчет – Flexible
        /// </summary>
        //Flexible = 37,

        /// <summary>
        /// Список доступных отчетов
        /// </summary>
        //AvailableReports = 500
    }
}
