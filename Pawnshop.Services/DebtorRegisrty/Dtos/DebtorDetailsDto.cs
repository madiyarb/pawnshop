using System;
using Pawnshop.Data.Models.DebtorRegistry.CourtOfficer;

namespace Pawnshop.Services.DebtorRegisrty.Dtos
{
    public class DebtorDetailsDto
    {
        public int Id { get; set; }

        /// <summary>
        /// ЧСИ
        /// </summary>
        public CourtOfficerDto? CourtOfficer { get; set; }

        /// <summary>
        /// В работе
        /// </summary>
        public bool InProcess { get; set; }

        /// <summary>
        /// ИИН клиента
        /// </summary>
        public string IdentityNumber { get; set; }

        /// <summary>
        /// номер протокола исп. производства
        /// </summary>
        public string? CourtExecProtocolNum { get; set; }

        /// <summary>
        ///  Орган, выдавший исполнительный документ
        /// </summary>
        public string? OrganisationName { get; set; }

        /// <summary>
        /// Название ДИСА
        /// </summary>
        public string? DisaName { get; set; }

        /// <summary>
        /// Сумма взыскания
        /// </summary>
        public decimal? RecoveryAmount { get; set; }

        public DateTimeOffset CreateDate { get; set; }

        /// <summary>
        /// Дата начала производства
        /// </summary>
        public DateTimeOffset? InitiationDate { get; set; }

        /// <summary>
        /// Дата возбуждения Дела
        /// </summary>
        public DateTimeOffset? StartExecutionDate { get; set; }

        /// <summary>
        /// Есть запрет на выезд
        /// </summary>
        public bool? IsTravelBan { get; set; }

        /// <summary>
        ///  Дата наложения запрета на выезд
        /// </summary>
        public DateTimeOffset? TravelBanStartDate { get; set; }
    }
}