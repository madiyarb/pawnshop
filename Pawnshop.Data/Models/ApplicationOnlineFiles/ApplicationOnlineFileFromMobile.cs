using Newtonsoft.Json;
using System;

namespace Pawnshop.Data.Models.ApplicationOnlineFiles
{
    public class ApplicationOnlineFileFromMobile
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string FileGuid { get; set; }
        public string Title { get; set; }
        [JsonIgnore]
        public DateTime CreateDate { get; set; }
    }
}
