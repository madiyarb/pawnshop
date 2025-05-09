using Pawnshop.Data.Models.Interaction;
using System;

namespace Pawnshop.Web.Models.Interaction
{
    public class InteractionCreateBinding
    {
        public InteractionType InteractionType { get; set; }
        public string InternalPhone { get; set; }
        public string ExternalPhone { get; set; }
        public string Result { get; set; }
        public string CarYear { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string PreferredLanguage { get; set; }
        public int? ClientId { get; set; }
        public Guid? ApplicationOnlineId { get; set; }
        public int? CallPurposeId { get; set; }
        public int? AttractionChannelId { get; set; }
        public int? CallId { get; set; }
        public int? SmsNotificationId { get; set; }
    }
}
