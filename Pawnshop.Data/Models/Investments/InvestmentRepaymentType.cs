using System;

namespace Pawnshop.Data.Models.Investments
{
    public enum InvestmentRepaymentType : short
    {
        /// <summary>
        /// Полное погашение начисленного процента
        /// </summary>
        All = 10,

        /// <summary>
        /// Частичное погашение начисленного процента
        /// </summary>
        Partial = 20,

        /// <summary>
        /// Без погашения начисленного процента
        /// </summary>
        None = 30
    }
}
