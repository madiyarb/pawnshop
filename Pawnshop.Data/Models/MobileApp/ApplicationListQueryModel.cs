using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ApplicationListQueryModel
    {
        public int? AuthorId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ApplicationStatus? Status { get; set; }
        public bool? IsAddition { get; set; }
        public bool ShowAllApplications { get; set; } = false;
    }
}
