using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.Calls
{
    public class Call
    {
        public int Id { get; set; }

        public string CallPbxId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public DateTime? DeleteDate { get; set; }

        public string PhoneNumber { get; set; }

        public int? ClientId { get; set; }

        public Client Client { get; set; }

        public string Status { get; set; }

        public string Duration { get; set; }

        public string Direction { get; set; }

        public string Language { get; set; }

        public string RecordFile { get; set; }

        public string UserInternalPhone { get; set; }

        public int? UserId { get; set; }

        public User User { get; set; }

        public string CompanyInternalPhone { get; set; }
    }
}
