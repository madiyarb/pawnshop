
namespace Pawnshop.Data.Models.LegalCollection.PrintTemplates
{
    public class LegalCasePrintTemplateModel
    {
        /// <summary>
        /// Данные займа
        /// </summary>
        public LegalCasePrintTemplateContractData ContractData { get; set; }
        /// <summary>
        /// Данные клиента
        /// </summary>
        public LegalCasePrintTemplateClientData ClientData { get; set; }
        /// <summary>
        /// Данные компаний выдавщий займ
        /// </summary>
        public LegalCasePrintTemplateCompanyData CompanyData { get; set; }

        public DebtCalculationPrintForm? DebtCalculationPrintForm { get; set; }
        public string? Signer { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
