using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Pawnshop.Data.Models.Dictionaries
{
    public interface IBodyNumber
    {
        /// <summary>
        /// Винкод
        /// </summary>
        public string BodyNumber { get; set; }
    }
}
