using System;

namespace Pawnshop.Web.Models.Calls
{
    public class CallView
    {
        public int Id { get; set; }

        public string CallPbxId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public string PhoneNumber { get; set; }

        public int? ClientId { get; set; }

        public string ClientName { get; set; }

        public string Status { get; set; }

        public string Duration { get; set; }

        public string Direction { get; set; }

        public string Language { get; set; }

        public string RecordFile { get; set; }

        public string UserInternalPhone { get; set; }

        public int? UserId { get; set; }

        public string UserName { get; set; }
    }
}
