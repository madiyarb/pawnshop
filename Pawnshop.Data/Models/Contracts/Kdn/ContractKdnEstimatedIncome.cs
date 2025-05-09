using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Kdn
{
    /// <summary>
    /// Доход, расчитанный от стоимости залогов
    /// </summary>
    public class ContractKdnEstimatedIncome : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        public int ContractId { get; set; }
        public int ContractPositionId { get; set; }
        public decimal PositionEstimatedIncome { get; set; }
        public decimal EstimatedIncome { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DeleteDate { get; set; }
        
        //Поля для отображения на стороне Фронта
        public string PositionDetails { get; set; }
        public bool IsGambler { get; set; }
    }
}
