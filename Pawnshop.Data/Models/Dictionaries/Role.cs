using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Роль
    /// </summary>
    public class Role : IDictionary
    {
        /// <summary>
        /// Идентификатор
        /// </summary>            
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Required(ErrorMessage = "Поле наименование обязательно для заполнения")]
        public string Name { get; set; }

        /// <summary>
        /// Права роли
        /// </summary>
        public PermissionListDefinition Permissions { get; set; }
    }
}