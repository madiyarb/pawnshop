using System;
using Pawnshop.Data.Models.DebtorRegistry.CourtOfficer;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.DebtorRegistry
{
    public class DebtRegistriesViewModel
    {
        public int Id { get; set; }
        
        /// <summary>
        /// номер протокола
        /// </summary>
        public string? ProtocolNumb { get; set; }
        
        /// <summary>
        /// Наличие запрета на выезд
        /// </summary>
        public bool? IsTravelBan { get; set; }
        
        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTimeOffset CreateDate { get; set; }
        
        /// <summary>
        /// Дата начала производства (<ilDate>)
        /// </summary>
        public DateTimeOffset? InitiationDate { get; set; }
        
        /// <summary>
        /// Дата возбуждения Дела (ipStartDate)
        /// </summary>
        public DateTimeOffset? StartExecutionDate { get; set; }
        
        /// <summary>
        /// Дата наложения запрета на выезд
        /// </summary>
        public DateTimeOffset? TravelBanStartDate { get; set; }
        
        /// <summary>
        /// Сумма взыскания
        /// </summary>
        public decimal? RecoveryAmount { get; set; }
        
        public ClientDto? Client { get; set; }
        
        /// <summary>
        /// ЧСИ
        /// </summary>
        public CourtOfficerDto? CourtOfficer { get; set; }
    }
}