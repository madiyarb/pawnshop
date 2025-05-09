using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pawnshop.Data.Models.Mintos
{
    public class MintosValidationResultModel : MintosValidationModel
    {
        public ValidationErrorCode ErrorCode { get; set; }
        public int? MintosContractId { get; set; }
        public int? ContractId { get; set; }
        public int? MintosId { get; set; }
        public string MintosStatus { get; set; }
        public DateTime? UploadDate { get; set; }
        public int? ContractActionId { get; set; }
    }
}