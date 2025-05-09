using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Positions
{
    public class PositionEstimateHistory : IEntity
    {
        public int Id {get;set;}

        public int CompanyId { get; set; }
        public Client Company { get; set; }
        public int PositionId { get; set; }
        public decimal CollateralCost { get; set; }
        public decimal EstimatedCost { get; set; }
        public int? FileRowId { get; set; }
        public DateTime Date { get; set; }
        public DateTime BeginDate { get; set; }
        public string Number { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
