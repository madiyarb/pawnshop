namespace Pawnshop.Data.Models.ApplicationsOnline.Events
{
    public sealed class ApplicationOnlineStatusChanged 
    {
        public string Type => nameof(ApplicationOnlineStatusChanged);

        public string Status { get; set; }
        public ApplicationOnline ApplicationOnline { get; set; }
        public ApplicationOnlineContractInfo? ApplicationOnlineContractInfo { get; set; }
    }
}
