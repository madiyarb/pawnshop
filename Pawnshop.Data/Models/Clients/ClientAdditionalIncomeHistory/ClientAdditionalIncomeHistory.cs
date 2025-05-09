namespace Pawnshop.Data.Models.Clients.ClientAdditionalIncomeHistory
{
    public class ClientAdditionalIncomeHistory : ClientAdditionalIncomeActualLog
    {
        public int RowNum { get; set; }
        public string ContractNumber { get; set; }
        public string OperationAuthorFullName { get; set; }
        public string ConfirmationDocumentName { get; set; }
    }
}
