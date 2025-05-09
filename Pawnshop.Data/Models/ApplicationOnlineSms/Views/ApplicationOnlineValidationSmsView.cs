namespace Pawnshop.Data.Models.ApplicationOnlineSms.Views
{
    public sealed class ApplicationOnlineValidationSmsView
    {
        public bool? Success { get; set; }
        public int RetryCount { get; set; }
        public int AttemptsLeft { get; set; }
    }
}
