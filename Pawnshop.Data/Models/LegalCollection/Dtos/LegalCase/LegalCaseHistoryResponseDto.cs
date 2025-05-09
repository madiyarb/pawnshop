using System;

namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class LegalCaseHistoryResponseDto
    {
        public int Id { get; set; }
        public int LegalCaseId { get; set; }
        /// <summary>
        /// Состояние(результат)
        /// </summary>
        public int LegalStageAfterId { get; set; }
        
        /// <summary>
        /// Суд
        /// </summary>
        public int? LegalCaseCourtId { get; set; }
        public int DelayDays { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset? DeleteDate { get; set; }
        
        /// <summary>
        /// Действие(Action) legal case
        /// </summary>
        public string? Action { get; set; }
        
        /// <summary>
        /// Результат(стадия)
        /// </summary>
        public string? StageAfter { get; set; }

        /// <summary>
        /// Автор дела сотрудника начавшего обработку дело 
        /// </summary>
        public int? AuthorId { get; set; }
        
        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }
    }
}