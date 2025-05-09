using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Mintos
{
    /// <summary>
    /// Черный список выгрузки в Mintos
    /// </summary>
    public class MintosBlackList : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Заблокирован до дата/время
        /// </summary>
        public DateTime LockUntilDate { get; set; } = DateTime.Now.AddDays(2);//по умолчанию - дата блокировки 2 суток
    }
}
