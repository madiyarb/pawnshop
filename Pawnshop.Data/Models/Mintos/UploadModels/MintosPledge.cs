using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos
{
    /// <summary>
    /// Залог - Авто
    /// </summary>
    public class MintosPledge
    {
        /// <summary>
        /// Вид залога
        /// Car Loan (pledge - vehicle)
        /// Mortgage Loan - Apartment(pledge-apartment)
        /// Mortgage Loan - Commercial property(pledge-commercial-property)
        /// Business Loan(pledge-company)
        /// Mortgage Loan - House(pledge-house)
        /// Personal Loan(pledge-unsecured)
        /// Short-Term Loan(pledge-payday)
        /// Invoice Financing(pledge-invoice)
        /// Mortgage Loan - Land(pledge-land)
        /// Agricultural Loan(pledge-agricultural)
        /// Pawnbroking Loan(pledge-other)
        /// </summary>
        public virtual string type { get; set; }
    }
}
