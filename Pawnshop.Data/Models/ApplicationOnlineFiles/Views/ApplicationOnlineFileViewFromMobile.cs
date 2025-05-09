using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationOnlineFiles.Views
{
    public class ApplicationOnlineFileViewFromMobile
    {
        public int ContractId { get; set; }
        public List<ApplicationOnlineFileFromMobile> Documents { get; set; }
    }
}
