using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class Realty : Position
    {
        public int RealtyTypeId { get; set; }
        [DisplayName("Кадастровый номер")]
        public string CadastralNumber { get; set; }
        public string? CadastralNumberAdditional { get; set; }
        [DisplayName("РКА")]
        public string Rca { get; set; }
        public int? Year { get; set; }
        public decimal? TotalArea { get; set; }
        public decimal? LivingArea { get; set; }
        public int? StoreysNumber { get; set; }
        public int? WallMaterial { get; set; }
        public int? RoomsNumber { get; set; }
        public int? LocationStoreysNumber { get; set; }
        public decimal? LandArea { get; set; }
        public decimal? LandAreaRatio { get; set; }
        public int? PurposeId { get; set; }
        public int? LightingId { get; set; }
        public int? ColdWaterSupplyId { get; set; }
        public int? GasSupplyId { get; set; }
        public int? SanitationId { get; set; }
        public int? HotWaterSupplyId { get; set; }
        public int? HeatingId { get; set; }
        public int? PhoneConnectionId { get; set; }
        public RealtyAddress? Address { get; set; }
        public List<RealtyDocument>? RealtyDocuments { get; set; }

        public DomainValue? RealtyType { get; set; }

        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        public int? ContractId { get; set; }
    }
}
