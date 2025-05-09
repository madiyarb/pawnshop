using System;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Models.ClientAdditionalContactLogItems
{
    public class ClientAdditionalContactLogData
    {
        public int ClientAdditionalContactId { get; set; }
        public int ClientId { get; set; }
        public string PhoneNumber { get; set; }
        public int ContactOwnerTypeId { get; set; }
        public string ContactOwnerFullname { get; set; }
        public DateTime? DeleteDate { get; set; }

        public ClientAdditionalContactLogData()
        {
            
        }

        public ClientAdditionalContactLogData(ClientAdditionalContact contact)
        {
            ClientAdditionalContactId = contact.Id;
            ClientId = contact.ClientId;
            PhoneNumber = contact.PhoneNumber;
            ContactOwnerTypeId = contact.ContactOwnerTypeId;
            ContactOwnerFullname = contact.ContactOwnerFullname;
            DeleteDate = contact.DeleteDate;
        }
    }
}
