using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Verification
{
    public class RegisterVerificationNumberResponse
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int VerificationId { get; set; }

        /// <summary>
        /// Дата истечения
        /// </summary>
        public DateTime ExpireDate { get; set; }
    }
}
