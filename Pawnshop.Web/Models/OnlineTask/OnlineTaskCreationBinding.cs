using System;

namespace Pawnshop.Web.Models.OnlineTask
{
    public sealed class OnlineTaskCreationBinding
    {
        public Guid? Id { get; set; }
        public string Type { get; set; }
        public string Decription { get; set; }
        public string ShortDescription { get; set; }
        public int? UserId { get; set; }
        public Guid? ApplicationId { get; set; }
        public int? ClientId { get; set; }
        public LeadBinding Lead { get; set; }
    }
}
