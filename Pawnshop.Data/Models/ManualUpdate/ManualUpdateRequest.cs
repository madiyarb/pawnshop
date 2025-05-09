using Pawnshop.Core;

namespace Pawnshop.Data.Models.ManualUpdate
{
    public class ManualUpdateRequest : IEntity
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string SelectQuery { get; set; }
        public string UpdateQuery { get; set; }
    }
}