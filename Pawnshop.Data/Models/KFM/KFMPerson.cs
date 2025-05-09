using System;

namespace Pawnshop.Data.Models.KFM
{
    public class KFMPerson
    {
        public int Num { get; set; }

        public string Surname { get; set; }

        public string Name { get; set; }

        public string Patronymic { get; set; }

        public DateTime? Birthdate { get; set; }

        public string IdentityNumber { get; set; }

        public string Note { get; set; }

        public string Correction { get; set; }

        public DateTime UploadDate { get; set; }
    }
}
