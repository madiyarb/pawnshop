namespace Pawnshop.Data.Models.Clients.ClientIncomeHistory
{
    public class ClientIncomeHistory : ClientIncomeLog
    {
        public int RowNum { get; set; }
        public string ContractNumber { get; set; }
        public string OperationAuthorFullName { get; set; }
        public string ConfirmationDocumentName { get; set; }
    }
}
