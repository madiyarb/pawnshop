namespace Pawnshop.Services.TasLabBankrupt
{
    public sealed class TasLabBankruptInfoServiceOptions
    {
        public string BaseUrl { get; set; }

        public string UserName { get; set; }

        public string Secret { get; set; }
        public int? TimeoutSeconds { get; set; } = 5;
    }
}
