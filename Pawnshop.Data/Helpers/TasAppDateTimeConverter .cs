using Newtonsoft.Json.Converters;

namespace Pawnshop.Data.Helpers
{
    class TasAppDateTimeConverter : IsoDateTimeConverter
    {
        public TasAppDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
