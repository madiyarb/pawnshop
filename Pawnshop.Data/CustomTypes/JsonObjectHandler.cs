using System;
using System.Data;
using Dapper;
using Newtonsoft.Json;

namespace Pawnshop.Data.CustomTypes
{
    public class JsonObjectHandler<T> : SqlMapper.TypeHandler<T> where T: class, IJsonObject
    {
        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value != null ? (object) JsonConvert.SerializeObject(value) : DBNull.Value;
        }

        public override T Parse(object value)
        {
            if (value == null || value is DBNull) return null;

            var s = value as string;
            if (string.IsNullOrWhiteSpace(s)) return null;
            return JsonConvert.DeserializeObject<T>(s);
        }
    }
}