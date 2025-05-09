namespace Pawnshop.Services.SUSN
{
    public sealed class TasLabSUSNServiceOptions
    {
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }
        public int? TimeoutSeconds { get; set; } = 5;
    }
}
