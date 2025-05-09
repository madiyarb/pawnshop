using System;
using System.Linq;

namespace Pawnshop.Core.Queries
{
    public static class QueryExtensions
    {
        public static string Like(this ListQuery query, string pre, params string[] fields)
        {
            if (string.IsNullOrWhiteSpace(query.Filter) && string.IsNullOrWhiteSpace(pre))
                return null;
            if (string.IsNullOrWhiteSpace(query.Filter))
            {
                return $"WHERE {pre}";
            }

            var preText = string.IsNullOrWhiteSpace(pre) ? "WHERE" : $"WHERE {pre} AND";
            return $"{preText} ({string.Join(" OR ", fields.Select(f => $"{f} LIKE '%' + @filter + '%'").ToArray())})";
        }
        public static string Order(this ListQuery query, string pre, Sort defaultSort = null)
        {
            var sort = query?.Sort ?? defaultSort;
            if (sort == null) return null;

            var preText = string.IsNullOrWhiteSpace(pre) ? "ORDER BY" : $"ORDER BY {pre},";
            return $"{preText} {sort.Name} {Enum.GetName(typeof(SortDirection), sort.Direction)}";
        }

        public static string Page(this ListQuery query)
        {
            if (query?.Page == null) return null;

            return "OFFSET (@offset) ROWS FETCH NEXT @limit ROWS ONLY";
        }
    }
}