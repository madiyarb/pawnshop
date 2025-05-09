using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public enum ProdKind : short
    {
        /// <summary>
        /// Аннуитет
        /// </summary>
        Light = 1,
        /// <summary>
        /// Дискрет
        /// </summary>
        Turbo = 2,
        /// <summary>
        /// Без права вождения
        /// </summary>
        Motor = 3
    }
}
