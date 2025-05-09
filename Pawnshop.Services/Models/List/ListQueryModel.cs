using Pawnshop.Core.Queries;

namespace Pawnshop.Services.Models.List
{
    public class ListQueryModel<T> : ListQuery
    {
        public T Model { get; set; }
    }
}