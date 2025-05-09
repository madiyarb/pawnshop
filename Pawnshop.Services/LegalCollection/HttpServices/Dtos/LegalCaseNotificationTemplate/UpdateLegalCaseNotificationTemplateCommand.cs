namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseNotificationTemplate
{
    public class UpdateLegalCaseNotificationTemplateCommand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string MessageTemplate { get; set; }
    }
}
