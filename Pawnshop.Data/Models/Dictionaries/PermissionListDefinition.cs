using Pawnshop.Data.CustomTypes;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class PermissionListDefinition : IJsonObject
    {
        public PermissionDefinition[] Permissions { get; set; }
    }
}