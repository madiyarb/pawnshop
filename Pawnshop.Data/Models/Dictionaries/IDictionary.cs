using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries
{
    public interface IDictionary : IEntity
    {
        string Name { get; set; }
    }
}