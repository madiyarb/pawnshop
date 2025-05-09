using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Audit
{
    public class EventLogItem : IEntity
    {
        public int Id { get; set; }

        public EventCode EventCode { get; set; }

        public EventStatus EventStatus { get; set; }

        public int? UserId { get; set; }

        public string UserName { get; set; }

        public int? BranchId { get; set; }

        public string BranchName { get; set; }

        public string Uri { get; set; }

        public string Address { get; set; }

        public EntityType? EntityType { get; set; }

        public int? EntityId { get; set; }

        public string RequestData { get; set; }

        public string ResponseData { get; set; }

        public DateTime CreateDate { get; set; }

        public string EventDescription
        {
            get => EventCode.GetDisplayName();
        }
    }
}