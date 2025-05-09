using Microsoft.AspNetCore.Http;
using System;

namespace Pawnshop.Web.Models.ApplicationOnlineApprovedOtherPayment
{
    public class ApprovedOtherPaymentCreateView
    {
        public Guid ApplicationOnlineId { get; set; }
        public decimal Amount { get; set; }
        public string SubjectName { get; set; }
        public IFormFile File { get; set; }
    }
}
