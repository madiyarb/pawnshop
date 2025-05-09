using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.TMF
{
    public class TEMPTMFBaseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }

        public object[] Result { get; set; }
    }
}
