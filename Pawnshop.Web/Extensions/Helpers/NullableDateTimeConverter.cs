using System.Text.Json;
using System;
using System.Text.Json.Serialization;

namespace Pawnshop.Web.Extensions.Helpers
{
    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TryGetDateTime(out DateTime value) ? (value.ToLocalTime()) : (DateTime?)null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToLocalTime());
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
