using Pawnshop.AccountingCore.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.TMF
{
    public class TMFRequest : Pawnshop.Core.IEntity
    {
        public int Id { get; set; }
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public TMFRequestStatus Status { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public DateTime DeleteDate { get; set; }
        public int? PaymentId { get; set; }
        public object? ResponseDataObject { get; set; }
        public object? ResponseDataResultObject { get; set; }
    }
}
