namespace Pawnshop.Data.Models.Localizations
{
    public class Localization
    {
        public int Id { get; set; }
        public int LocalizationItemId { get; set; }
        public string Language { get; set; }
        public string Value { get; set; }
    }
}
