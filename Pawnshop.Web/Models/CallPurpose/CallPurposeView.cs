using System;

namespace Pawnshop.Web.Models.CallPurpose
{
    public class CallPurposeView
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Title { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int? UpdateAuthorId { get; set; }
        public string UpdateAuthorName { get; set; }
    }
}
