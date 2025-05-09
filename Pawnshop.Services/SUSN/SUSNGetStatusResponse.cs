using System.Collections.Generic;


namespace Pawnshop.Services.SUSN
{
    public sealed class SUSNGetStatusResponse
    {
        public string iin { get; set; }
        public string consentToken { get; set; }
        public List<SusnGetStatusResponseStatuses> status { get; set; }
    }
}
