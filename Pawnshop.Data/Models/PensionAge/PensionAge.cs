using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.PensionAge
{
    public class PensionAge : IEntity
    {
        public int Id { get; set; }

        public double Age { get; set; }

        public bool IsMale { get; set; }

        public DateTime ActivationDate { get; set; }

        public DateTime CreateDate { get; set; }

        public int AuthorId { get; set; }
        public DateTime? DeleteDate { get; set; }

    }
}