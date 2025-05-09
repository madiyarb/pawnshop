using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries
{
    public interface ITranslatedDictionary : IEntity
    {
        string NameRus { get; set; }
        string NameKaz { get; set; }
    }
}