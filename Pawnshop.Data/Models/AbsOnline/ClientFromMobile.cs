using System;

namespace Pawnshop.Data.Models.AbsOnline
{
    public class ClientFromMobile
    {
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public DateTime? BirthDay { get; set; }
        public bool? IsMale { get; set; }
        public ClientDocumentFromMobile Document { get; set; }
    }
}
