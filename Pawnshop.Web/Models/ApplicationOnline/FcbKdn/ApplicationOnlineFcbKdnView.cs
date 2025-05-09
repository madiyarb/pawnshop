using System;

namespace Pawnshop.Web.Models.ApplicationOnline.FcbKdn
{
    public class ApplicationOnlineFcbKdnView
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        public string CreateByName { get; set; }
        public decimal PaymentAmount { get; set; }
        public bool Success { get; set; }
    }
}
