using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class PositionEstimate : IEntity
    {
        public int Id { get; set; }
        public int PositionId { get; set; }
        public int? CompanyId { get; set; }
        public DateTime? Date { get; set; }
        public string? Number { get; set; }
        public int? FileRowId { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public DateTime? DeleteDate { get; set; }
        public Client? Company { get; set; }
    }
}
