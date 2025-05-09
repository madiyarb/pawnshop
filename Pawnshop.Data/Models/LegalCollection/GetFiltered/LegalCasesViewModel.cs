using System;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.LegalCollection
{
    public class LegalCasesViewModel
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string LegalCaseNumber { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        
        /// <summary>
        /// Причина передачи в Legal
        /// </summary>
        public string? Reason { get; set; }
        
        /// <summary>
        /// Статус дела Legal Collection
        /// </summary>
        public int StatusId { get; set; }
        
        /// <summary>
        /// Направление дела Legal Collection
        /// </summary>
        public int? CourseId { get; set; }
        
        /// <summary>
        /// Стадия дела Legal Collection
        /// </summary>
        public int? StageId { get; set; }
        
        public string ContractNumber { get; set; }
        public LegalCaseContractInfoDto ContractData { get; set; }
        
        /// <summary>
        /// текущий день просрочки
        /// </summary>
        public int? DelayCurrentDay { get; set; }
        
        /// <summary>
        /// Количество дней до выполнения задачи в рамках контроля сроов
        /// </summary>
        public int? DaysUntilExecution { get; set; }
        
        /// <summary>
        /// Статус выполнения задачи в рамках контроля сроов
        /// </summary>
        public int? CaseTaskStatusId { get; set; }
        
        /// <summary>
        /// Наличие исп. производства у ЧСИ по клиенту договора этого дела
        /// </summary>
        public bool HasRecoveryProcessByContractIin { get; set; }
    }
}