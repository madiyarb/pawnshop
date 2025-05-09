using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Вид продукта
    /// </summary>
    public class LoanProductType : IDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAlt { get; set; }
        [Required(ErrorMessage = "Уникальный код обязателен к заполнению")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Вид залога обязателен к заполнению")]
        public CollateralType CollateralType { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }

        public List<LoanProductTypeAccount> Accounts { get; set; }
    }
}
