using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class RealtyAddress : IEntity
    {
        public int Id { get; set; }
        public int AteId { get; set; }
        public int GeonimId { get; set; }
        public string? BuildingNumber { get; set; }
        public string? BuildingAdditionalNumber { get; set; }
        public string? AppartmentNumber { get; set; }
        public string? FullPathRus { get; set; }
        public string? FullPathKaz { get; set; }
    }
}
