using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ApplicationOnlineLog
{
    [Table("ApplicationOnlineLogItems")]
    public sealed class ApplicationOnlineLogItem : ApplicationOnlineLogData
    {
        [ExplicitKey]
        public Guid Id { get; set; }    
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }

        public ApplicationOnlineLogItem()
        {
            
        }

        public ApplicationOnlineLogItem(ApplicationOnlineLogData data, int? userId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            UserId = userId;
            ApplicationId = data.ApplicationId;
            ProductId = data.ProductId;
            LoanTerm = data.LoanTerm;
            ApplicationAmount = data.ApplicationAmount;
        }
    }
}
