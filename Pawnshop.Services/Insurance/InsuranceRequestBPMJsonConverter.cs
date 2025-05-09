using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Services.Insurance
{
    public class InsuranceRequestBPMJsonConverter : JsonConverter
    {
        private readonly Type[] _types;

        public InsuranceRequestBPMJsonConverter(params Type[] types)
        {
            _types = types;
        }

        public override bool CanConvert(Type objectType)
        {
            return _types.Any(t => t == objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is InsuranceCreatePolicyRequestBPM createPolicyRequest)
            {
                WriteVariables(writer, createPolicyRequest.variables);
            }
            else if (value is InsuranceCancelPolicyRequestBPM cancelPolicyRequest)
            {
                WriteVariables(writer, cancelPolicyRequest.variables);
            }
        }

        private void WriteVariables(JsonWriter writer, InsuranceCreatePolicyRequestVariablesBPM variables)
        {
            WriteVariablesCore(writer, variables);
        }

        private void WriteVariables(JsonWriter writer, InsuranceCancelPolicyRequestVariablesBPM variables)
        {
            WriteVariablesCore(writer, variables);
        }

        private void WriteVariablesCore<T>(JsonWriter writer, T variables)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("variables");
            writer.WriteStartObject();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var propertyName = GetJsonPropertyName(propertyInfo);
                var propertyValue = (string)propertyInfo.GetValue(variables);

                writer.WritePropertyName(propertyName);
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteValue(propertyValue);
                writer.WritePropertyName("type");
                writer.WriteValue("string");
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        private string GetJsonPropertyName(PropertyInfo propertyInfo)
        {
            var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonPropertyAttribute != null)
            {
                return (jsonPropertyAttribute).PropertyName;
            }
            return propertyInfo.Name;
        }
    }
}
