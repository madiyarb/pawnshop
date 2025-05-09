using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries.Address
{
    public class AddressRoom : IEntity, IAddressEGOV
    {
        public int Id { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomTypeId { get; set; }
        public string FullPathRus { get; set; }
        public string FullPathKaz { get; set; }
        public string Number { get; set; }
        public bool? IsActual { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string RCACode { get; set; }
    }
}
