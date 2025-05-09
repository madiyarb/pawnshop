using System;

namespace Pawnshop.Web.Models.ApplicationOnlineCheck
{
    public class ApplicationOnlineCheckView
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        public string CreateByName { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateBy { get; set; }
        public string UpdateByName { get; set; }
        public Guid ApplicationOnlineId { get; set; }
        public int TemplateId { get; set; }
        public string CheckName { get; set; }
        public bool Checked { get; set; }
        public string Note { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
