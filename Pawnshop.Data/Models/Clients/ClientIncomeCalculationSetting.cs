using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Основные доходы клиента
    /// </summary>
    public class ClientIncomeCalculationSetting : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        public int DocumentTypeId { get; set; }
        public decimal Rate { get; set; }
    }
}
