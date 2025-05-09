using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ReportData
{
    public class ReportData : IEntity
    {
        public ReportData(int id, int organizationId, int branchId, DateTime date,List<ReportDataRow> rows)
        {
            Id = id;
            OrganizationId = organizationId;
            BranchId = branchId;
            Date = date;
            Rows = rows;
        }

        public ReportData(int id, int organizationId, int branchId, DateTime date, DateTime deleteDate)
        {
            Id = id;
            OrganizationId = organizationId;
            BranchId = branchId;
            Date = date;
            DeleteDate = deleteDate;
        }

        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public int BranchId { get; set; }
        public DateTime Date { get; set; }
        public List<ReportDataRow> Rows { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
