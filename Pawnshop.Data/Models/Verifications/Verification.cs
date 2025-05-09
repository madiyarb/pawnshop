using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Verifications
{
    public class Verification : IEntity
    {
        public int Id { get; set; }
        public string OTP { get; set; }
        public string Address { get; set; }
        public int ClientId { get; set; }
        public int TryCount { get; set; }
        public int MaxTryCount { get; set; }
        public int AuthorId { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
