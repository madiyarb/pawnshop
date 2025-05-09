using System;

namespace Pawnshop.Data.Models.Clients
{
    public sealed class ApplicationOnlineClient
    {

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronymic { get; set; }
        public string? IdentityNumber { get; set; }
        public DateTime? BirthDay { get; set; }
        public bool? IsMale { get; set; }
        public string? EMail { get; set; }
        public string? PartnerCode { get; set; }
        public bool? ReceivesASP { get; set; }

        public string? FullName { get; set; }

        public ApplicationOnlineClient() { }

        public void Update(string? name, string? surname, string? patronymic, string? identityNumber,
            DateTime? birthDay, bool? isMale, string? eMail, string? partnerCode, bool? recieveAsp)
        {
            if (!string.IsNullOrEmpty(name) && Name != name)
            {
                Name = name;
            }

            if (!string.IsNullOrEmpty(surname) && Surname != surname)
            {
                Surname = surname;
            }

            if (!string.IsNullOrEmpty(patronymic) && Patronymic != patronymic)
            {
                Patronymic = patronymic;
            }

            if (!string.IsNullOrEmpty(identityNumber) && IdentityNumber != identityNumber)
            {
                IdentityNumber = identityNumber;
            }

            if (birthDay.HasValue && birthDay != BirthDay)
            {
                BirthDay = birthDay;
            }

            if (isMale.HasValue && IsMale != isMale)
            {
                IsMale = isMale;
            }

            if (!string.IsNullOrEmpty(eMail) && EMail != eMail)
            {
                EMail = eMail;
            }

            if (!string.IsNullOrEmpty(partnerCode) && PartnerCode != partnerCode)
            {
                PartnerCode = partnerCode;
            }

            if (recieveAsp.HasValue && ReceivesASP != recieveAsp)
            {
                ReceivesASP = recieveAsp;
            }

            string fullName = Surname + " " + Name + " " + Patronymic; // we can change some of fields actual model now in this entity
            if (!string.IsNullOrEmpty(fullName) && FullName != fullName)
            {
                FullName = fullName;
            }
        }
    }
}
