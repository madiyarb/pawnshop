using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class Holiday : IEntity
    {
        public int Id { get; set; }
        [RequiredDate(ErrorMessage = "Дата праздника обязательна к заполнению")]
        public DateTime Date { get; set; }
        [RequiredDate(ErrorMessage = "Дата оплаты обязательна к заполнению")]
        public DateTime PayDate { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
