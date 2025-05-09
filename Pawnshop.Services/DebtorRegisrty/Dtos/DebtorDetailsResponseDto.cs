using System;
using Pawnshop.Data.Models.DebtorRegistry.CourtOfficer;

namespace Pawnshop.Services.DebtorRegisrty.Dtos
{
    public class DebtorDetailsResponseDto
    {
        /// <summary>
        /// исп. пр-во номер
        /// </summary>
        public string? ExecProcNum { get; set; }
        
        /// <summary>
        /// дата возбуждения исп. пр-ва
        /// </summary>
        public DateTimeOffset? IpStartDate { get; set; }

        /// <summary>
        /// Дата начала производства
        /// </summary>
        public DateTimeOffset? IlDate { get; set; }
        
        /// <summary>
        ///  сумма взыскания
        /// </summary>
        public decimal? RecoveryAmount { get; set; }
        
        /// <summary>
        /// БИН взыскателя
        /// </summary>
        public string? RecovererBin { get; set; }
        
        /// <summary>
        ///  Наименование взыскателя
        /// </summary>
        public string? RecovererTitle { get; set; }
        
        /// <summary>
        ///  запрет на выезд
        /// </summary>
        public bool? IsBanned { get; set; }
        
        /// <summary>
        ///  Дата наложения запрета на выезд
        /// </summary>
        public string? BanStartDate { get; set; }
        
        /// <summary>
        /// ЧСИ
        /// </summary>
        public CourtOfficerDto? CourtOfficer { get; set; }
    }
}