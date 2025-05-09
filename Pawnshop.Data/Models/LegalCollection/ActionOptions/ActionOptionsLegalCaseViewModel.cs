using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Data.Models.LegalCollection.PrintTemplates;

namespace Pawnshop.Data.Models.LegalCollection.Action
{
    /// <summary>
    /// Данные для модального окна действий с делом(legal case)
    /// </summary>
    public class ActionOptionsLegalCaseViewModel
    {
        /// <summary>
        /// Суд
        /// </summary>
        public string? CaseCourt { get; set; }
        public int? CaseCourtId { get; set; }
        
        /// <summary>
        /// возможные действия с делом согласно справочника сценариев
        /// </summary>
        public List<LegalCaseActionDto> Actions { get; set; }
        
        /// <summary>
        /// возможные направления дела согласно справочника сценариев
        /// </summary>
        public List<LegalCaseCourseDto> Courses { get; set; }
        
        /// <summary>
        /// Суды
        /// </summary>
        public List<CourtDto> Courts { get; set; }
        
        /// <summary>
        /// Суммы на счетах у договора
        /// </summary>
        public List<ContractAmountsInfoDto> Amounts { get; set; }

        /// <summary>
        /// Сумма(долга)
        /// </summary>
        public decimal TotalCost { get; set; }
        
        /// <summary>
        /// Гос. пошлина
        /// </summary>
        public decimal StateFeeAmount { get; set; }
        
        /// <summary>
        /// Можно ли передавать/записывать гос. пошлину
        /// </summary>
        public bool CanPassStateFeeAmount { get; set; }
    }
}