using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.LegalCollection.Dtos.LegalCase
{
    public class LegalCaseNotificationHistoryDto
    {
        public int Id { get; set; }
        public int LegalCaseId { get; set; }
        public string MessageText { get; set; }
        public int CreateBy { get; set; }
        public User Author { get; set; }
        public DateTimeOffset? CreateDate { get; set; }
    }
}
