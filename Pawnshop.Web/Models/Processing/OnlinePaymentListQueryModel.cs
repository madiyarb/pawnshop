using System;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Web.Models.Processing
{
    public class OnlinePaymentListQueryModel
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }
        public ProcessingType? ProcessingType { get; set; }
    }
}