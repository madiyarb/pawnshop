using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Collection
{
    public class CollectionReason
    {
        public int? Id { get; set; }
        public string reasonName {  get; set; }
        public string reasonCode { get; set; }
        public string reasonType { get; set; }
        public int value { get; set; }
        public bool autoChange { get; set; }
        public int collateralType { get; set; }
    }
}
