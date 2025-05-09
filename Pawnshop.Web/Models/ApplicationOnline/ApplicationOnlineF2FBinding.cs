using System;

namespace Pawnshop.Web.Models.ApplicationOnline
{
    public sealed class ApplicationOnlineF2FBinding
    {
        public decimal Similarity { get; set; }
        public Guid DocFileId { get; set; }

        public string DocFileType { get; set; }
        public Guid CamFileId { get; set; }
        public string CamFileType { get; set;}
    }
}
