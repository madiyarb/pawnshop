using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public class ApplicationOnlineCheck : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        [JsonIgnore]
        public User Author { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateBy { get; set; }
        [JsonIgnore]
        public User UpdateAuthor { get; set; }
        public DateTime? DeleteDate { get; set; }
        public Guid ApplicationOnlineId { get; set; }
        public int TemplateId { get; set; }
        [JsonIgnore]
        public ApplicationOnlineTemplateCheck TemplateCheck { get; set; }
        public bool Checked { get; set; }
        public string Note { get; set; }
        public string AdditionalInfo { get; set; }

    }
}
