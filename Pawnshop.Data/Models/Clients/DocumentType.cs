using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Тип документа удостоверяющего личность
    /// </summary>
    public enum DocumentType : short
    {
        /// <summary>
        /// Удостоверение личности
        /// </summary>
        [Display(Name = "Удостоверение личности")]
        IdentityCard = 10,
        /// <summary>
        /// Паспорт
        /// </summary>
        [Display(Name = "Паспорт")]
        Passport = 20,
        /// <summary>
        /// Свидетельство о государственной регистрации юридического лица
        /// </summary>
        [Display(Name = "Свидетельство о государственной регистрации юридического лица")]
        LegalEntityRegistration = 30,
        /// <summary>
        /// Другое
        /// </summary>
        [Display(Name = "Другой документ")]
        Another = 40
    }
}