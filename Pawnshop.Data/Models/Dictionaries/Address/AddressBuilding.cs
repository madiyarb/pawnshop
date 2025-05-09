using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries.Address
{
    public class AddressBuilding : IEntity, IAddressEGOV
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int? ATEId { get; set; }
        public int? GeonimId { get; set; }
        public int? BuildingTypeId { get; set; }
        public string FullPathRus { get; set; }
        public string FullPathKaz { get; set; }
        public int? Number { get; set; }
        public bool? IsActual { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string RCACode { get; set; }
        public string ParentRCACode { get; set; }
    }
}
