using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Web.Models.Contract
{
    /// <summary>
    /// Критерии поиска внешней информации о договоре
    /// </summary>
    public class ContractOuterQueryModel
    {
        /// <summary>
        /// Номер договора
        /// </summary>
        [Required(ErrorMessage = "Поле номер договора обязательно для заполнения")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// ИИН
        /// </summary>
        [Required(ErrorMessage = "Поле идентификационный номер обязательно для заполнения")]
        public string IdentityNumber { get; set; }
    }
}
