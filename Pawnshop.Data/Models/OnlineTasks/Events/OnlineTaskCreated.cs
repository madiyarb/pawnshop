using System;

namespace Pawnshop.Data.Models.OnlineTasks.Events
{
    public class OnlineTaskCreated
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public int CreationUserId { get; set; }
        public int? UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Done { get; set; }
        public string Status { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public Guid? ApplicationId { get; set; }
        public int? ClientId { get; set; }
    }
}
