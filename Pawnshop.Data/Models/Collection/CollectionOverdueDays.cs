using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Collection
{
    public class CollectionOverdueDays
    {
        /// <summary>
        /// Количество дней для передачи в Soft Collection
        /// </summary>
        public int SoftCollection { get; set; }

        public int HardCollection { get; set; }
        
        /// <summary>
        /// Количество дней для стандартной передачи в LegalHard Collection
        /// </summary>
        public int Legalhard { get; set; }

        /// <summary>
        /// Количество дней для передачи недвижимости (CollateralType 60) в LegalHard Collection
        /// </summary>
        public int LegalhardByRealestate { get; set; }
    }
}
