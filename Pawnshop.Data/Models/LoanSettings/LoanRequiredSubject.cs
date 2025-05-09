using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.LoanSettings
{
    public class LoanRequiredSubject : IEntity
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public LoanSubject Subject { get; set; }
        public int SettingId { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
    }
}
