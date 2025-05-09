using Pawnshop.Data.Models.Base;
using System.Collections.Generic;
using System;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class PayType : IDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OperationCode { get; set; }
        public int? RequiredRequisiteTypeId { get; set; }
        public bool RequisiteIsRequired => RequiredRequisiteTypeId.HasValue;
        public int AccountId { get; set; }
        public bool IsDefault { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool AccountantUploadRequired { get; set; }
        public UseSystemType UseSystemType { get; set; }
        public List<PayTypeContractAction> Rules { get; set; }
    }
}
