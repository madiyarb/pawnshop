using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Mintos
{
    public class MintosValidationModel : IEntity
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string ContractNumber => new Regex(".+_").Replace(Number, string.Empty);
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}