using Pawnshop.Data.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Models.Contracts
{
    public class GoldContractSpecific : IJsonObject
    {
        /// <summary>
        /// Общий вес залога
        /// </summary>
        public double CollateralTotalWeight { get; set; }

        /// <summary>
        /// Удельный вес залога
        /// </summary>
        public double CollateralSpecificWeight { get; set; }

        /// <summary>
        /// Проба
        /// </summary>
        public int PurityId { get; set; }
    }
}
