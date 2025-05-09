namespace Pawnshop.Web.Models.ApplicationOnlineTemplateCheck
{
    public class ApplicationOnlineTemplateCheckUpdateView
    {
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
