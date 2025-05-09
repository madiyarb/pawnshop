using System;

namespace Pawnshop.Data.Models.ClientsMobilePhoneContacts
{
    public sealed class ClientsMobilePhoneContact
    {
        public Guid Id { get; set; }

        public string PhoneNumber { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public int ClientId { get; set; }

        public ClientsMobilePhoneContact()
        {
            
        }

        public ClientsMobilePhoneContact(string phoneNumber, string name, int clientId)
        {
            Id = Guid.NewGuid();
            PhoneNumber = phoneNumber;
            Name = name;
            ClientId = clientId;
            CreateDate = DateTime.UtcNow;
        }
    }
}
