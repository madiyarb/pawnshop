using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Audit
{
    public class JobLogItem : IEntity
    {
        public int Id { get; set; }

        public string JobName { get; set; }

        public JobCode JobCode { get; set; }

        public JobStatus JobStatus { get; set; }

        public EntityType EntityType { get; set; }

        public int? EntityId { get; set; }

        public string RequestData { get; set; }

        public string ResponseData { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
