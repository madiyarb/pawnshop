using System;

namespace Pawnshop.Data.Models.LegalCollection.Details
{
    public class LegalCaseHistoryViewModel
    {
        public DateTimeOffset? Date { get; set; }
        public string? Action { get; set; }
        public string? StageAfter { get; set; }
        public int? DelayDay { get; set; }
        public string? AuthorFullName { get; set; }
        public string? Note { get; set; }
    }
}