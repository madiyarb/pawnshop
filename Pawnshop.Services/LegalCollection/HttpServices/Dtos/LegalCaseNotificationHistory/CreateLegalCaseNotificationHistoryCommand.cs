namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseNotificationHistory
{
    public class CreateLegalCaseNotificationHistoryCommand
    {
        public string NotificationTemplateCode { get; set; }
        public int CreatedBy { get; set; }
        public int LegalCaseId { get; set; }
    }
}
