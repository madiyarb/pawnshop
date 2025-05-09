using System;

namespace Pawnshop.Web.Models.ApplicationOnlineApprovedOtherPayment
{
    public class ApprovedOtherPaymentView
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        public string CreateByName { get; set; }
        public string SubjectName { get; set; }
        public decimal Amount { get; set; }
        public Guid? FileId { get; set; }
        public string FileUrl { get; set; }
    }
}
