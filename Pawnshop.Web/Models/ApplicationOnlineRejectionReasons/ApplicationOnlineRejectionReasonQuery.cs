namespace Pawnshop.Web.Models.ApplicationOnlineRejectionReasons
{
    public sealed class ApplicationOnlineRejectionReasonQuery
    {
        public bool? ForManager { get; set; }
        public bool? ForClient { get; set; }
        public bool? Enabled { get; set; }
        public string? Code { get; set; }
    }
}
