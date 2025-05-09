using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class LoanSubject : IDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAlt { get; set; }
        [Required(ErrorMessage = "Уникальный код обязателен к заполнению")]
        public string Code { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? CBId { get; set; }
    }
}
