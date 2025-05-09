using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.OnlineTasks
{
    [System.ComponentModel.DataAnnotations.Schema.Table("OnlineTasks")]
    public sealed class OnlineTask
    {
        [ExplicitKey]
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

        public int? CallId { get; set; }
        public Guid? LeadId { get; set; }

        public OnlineTask() { }

        public OnlineTask(Guid? id, string type, int creationUserId, string description,
            string shortDescription, int? userId, int? clientId, Guid? applicationId = null, int? callId = null, Guid? leadId = null)
        {
            if (id == null)
            {
                Id = Guid.NewGuid();
            }
            else
            {
                Id = id.Value;
            }
            CreationUserId = creationUserId;
            Description = description;
            ShortDescription = shortDescription;
            Type = type;
            CreateDate = DateTime.Now;
            Done = false;
            Status = OnlineTaskStatus.Created.ToString();
            UserId = userId;
            ClientId = clientId;
            ApplicationId = applicationId;
            CallId = callId;
            LeadId = leadId;
        }

        public OnlineTask(Guid? id, string type, int creationUserId, string description,
            string shortDescription, Guid leadId)
        {
            if (id == null)
            {
                Id = Guid.NewGuid();
            }
            else
            {
                Id = id.Value;
            }
            CreationUserId = creationUserId;
            Description = description;
            ShortDescription = shortDescription;
            Type = type;
            CreateDate = DateTime.Now;
            Done = false;
            Status = OnlineTaskStatus.Created.ToString();
            LeadId = leadId;
        }
        public void Processing(int userId)
        {
            UserId = userId;
            Status = OnlineTaskStatus.Processing.ToString();
        }

        public void Complete(int? userId = null)
        {
            Done = true;
            Status = OnlineTaskStatus.Completed.ToString();
            CompleteDate = DateTime.Now;
            UserId = userId;
        }
    }
}
