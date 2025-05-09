using System;

namespace Pawnshop.Web.Models.Interaction
{
    public class InteractionView
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int? UpdateAuthorId { get; set; }
        public string UpdateAuthorName { get; set; }
        public int? ClientId { get; set; }
        public string InteractionTypeName { get; set; }
        public string PreferredLanguage { get; set; }
        public string InternalPhone { get; set; }
        public string ExternalPhone { get; set; }
        public string SmsMessage { get; set; }
        public string MainPhone { get; set; }
        public string AdditionalPhone { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public DateTime? BirthDay { get; set; }
        public int? AttractionChannelId { get; set; }
        public string AttractionChannelName { get; set; }
        public Guid? ApplicationOnlineId { get; set; }
        public int? CallPurposeId { get; set; }
        public string CallPurposeName { get; set; }
        public string IIN { get; set; }
        public string CarYear { get; set; }
        public string Result { get; set; }
    }
}
