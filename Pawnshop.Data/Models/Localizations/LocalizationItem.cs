using System.Collections.Generic;

namespace Pawnshop.Data.Models.Localizations
{
    public class LocalizationItem
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public string Code { get; set; }

        public List<Localization> Localizations { get; set; }
    }
}
