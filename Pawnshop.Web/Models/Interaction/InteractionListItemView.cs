using System;

namespace Pawnshop.Web.Models.Interaction
{
    public class InteractionListItemView
    {
        public int Id { get; set; }
        public int? AttractionChannelId { get; set; }
        public string AttractionChannelName { get; set; }
        public string ClientFullName { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreateDate { get; set; }
        public string ExternalPhone { get; set; }
        public string InternalPhone { get; set; }
        public string SmsMessage { get; set; }
        public string Result { get; set; }
        public string CallStatus { get; set; }
        public string RecordFileUrl { get; set; }
        public string InteractionType { get; set; }
    }
}
