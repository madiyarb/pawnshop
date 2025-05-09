using System;

namespace Pawnshop.Data.Models.ApplicationsOnline.Views
{
    public sealed class ApplicationOnlineClientView
    {
        public string IIN { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public DateTime? BirthDay { get; set; }
        public bool? IsMale { get; set; }
        public string Email { get; set; }
    }
}
