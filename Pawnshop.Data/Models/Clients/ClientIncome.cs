using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Data.Models.Files;
using System;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Основные доходы клиента
    /// </summary>
    public class ClientIncome : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        public int ClientId { get; set; }
        public int? ContractId { get; set; }
        public IncomeType IncomeType { get; set; }
        public int ConfirmationDocumentTypeId { get; set; }
        public int FileRowId { get; set; }
        [Write(false)]
        public FileRow FileRow { get; set; }
        public decimal IncomeTurns { get; set; }
        public decimal MonthQuantity { get; set; }
        public decimal IncomeAmount { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? DeleteDate { get; set; }
        public int AuthorId { get; set; }
    }
}
