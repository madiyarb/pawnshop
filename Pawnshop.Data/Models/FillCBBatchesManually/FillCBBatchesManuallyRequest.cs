using System;
using Microsoft.AspNetCore.Http;

namespace Pawnshop.Data.Models.FillCBBatchesManually
{
    public class FillCBBatchesManuallyRequest
    {
        public DateTime date { get; set; }
        public string cbs { get; set; }
        public IFormFile file { get; set; }
        public string contractIds { get; set; }
        public string isDaily { get; set; }

        private bool _isDaily;
        public bool IsDaily
        {
            get { return _isDaily; }
            set { _isDaily = isDaily == "true" ? true : false; }
        }
    }


}
