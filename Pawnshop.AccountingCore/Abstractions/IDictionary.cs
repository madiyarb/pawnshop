using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface IDictionary : IEntity
    {
        /// <summary>
        /// Наименование
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Наименование на втором языке
        /// </summary>
        string NameAlt { get; set; }
        /// <summary>
        /// Уникальный код
        /// </summary>
        string Code { get; set; }
    }
}
