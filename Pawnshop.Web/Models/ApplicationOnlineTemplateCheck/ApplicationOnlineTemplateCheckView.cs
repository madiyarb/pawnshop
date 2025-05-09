using System;

namespace Pawnshop.Web.Models.ApplicationOnlineTemplateCheck
{
    public class ApplicationOnlineTemplateCheckView
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        public string CreateByName { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateBy { get; set; }
        public string UpdateByName { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public bool IsActual { get; set; }
        public bool IsManual { get; set; }
        public int? Stage { get; set; }
        public bool ToVerificator { get; set; }
        public bool ToManager { get; set; }
        public bool ToTranche { get; set; }
        public string AttributeName { get; set; }
        public string AttributeCode { get; set; }
    }
}
