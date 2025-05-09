using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Insurances
{
    public class CancelInsuranceResponse
    {
        public bool success { get; set; }
        public Response response { get; set; }
        public string message { get; set; }
        public string errorResponse { get; set; }

        public class Response
        {
            public string response { get; set; }
            public bool isSuccess { get; set; }
            public DateTime timestamp { get; set; }
            public int status { get; set; }
            public string error { get; set; }
            public string message { get; set; }
            public string token {  get; set; }
        }
    }
}
