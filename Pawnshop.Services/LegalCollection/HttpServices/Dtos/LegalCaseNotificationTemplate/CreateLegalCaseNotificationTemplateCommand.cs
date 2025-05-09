
namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseNotificationTemplate
{
    public class CreateLegalCaseNotificationTemplateCommand
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string MessageTemplate { get; set; }
    }
}
