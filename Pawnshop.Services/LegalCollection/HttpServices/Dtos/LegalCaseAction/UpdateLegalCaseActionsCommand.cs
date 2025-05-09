namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseAction
{
    public class UpdateLegalCaseActionsCommand
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public string ActionCode { get; set; }
    }
}