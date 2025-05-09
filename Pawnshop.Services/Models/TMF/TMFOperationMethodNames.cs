using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace Pawnshop.Services.Models.TMF
{
    public enum TMFOperationMethodNames
    {
        [Display(Name = "CheckAccount")]
        CheckAccount = 10,
        [Display(Name = "Payment")]
        Payment = 20
    }
}
