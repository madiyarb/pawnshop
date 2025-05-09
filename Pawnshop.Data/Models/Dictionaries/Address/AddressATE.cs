using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries.Address
{
    public class AddressATE : ITranslatedDictionary, IAddressEGOV
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int? ATETypeId { get; set; }
        public string FullPathRus { get; set; }
        public string FullPathKaz { get; set; }
        public string NameRus { get; set; }
        public string NameKaz { get; set; }
        public int? KATOCode { get; set; }
        public bool? IsActual { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string RCACode { get; set; }
    }
}
