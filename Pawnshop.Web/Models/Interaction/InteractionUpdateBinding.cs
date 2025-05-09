using System;

namespace Pawnshop.Web.Models.Interaction
{
    public class InteractionUpdateBinding
    {
        public string Result { get; set; }
        public string CarYear { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string PreferredLanguage { get; set; }
        public int? CallPurposeId { get; set; }
        public int? AttractionChannelId { get; set; }
    }
}
