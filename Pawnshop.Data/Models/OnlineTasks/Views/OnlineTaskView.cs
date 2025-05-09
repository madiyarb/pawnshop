using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.OnlineTasks.Views
{
    public sealed class OnlineTaskView
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string TypeCode { get; set; }

        public int CreationUserId { get; set; }

        public int? UserId { get; set; }

        public bool Done { get; set; }

        public Guid? ApplicationId { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public string CreationAuthor { get; set; }

        public int? ClientId { get; set; }

        public string? ApplicationNumber { get; set; }

        public string Status { get; set; }

        public string Worker { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? CompleteDate { get; set; }

        public string ClientName { get; set; }

        public decimal ApplicationAmount { get; set; }

        public string LeadName { get; set; }

        public string LeadPatronymic { get; set; }

        public string LeadSurname { get; set; }

        public string LeadPhone { get; set; }

        public Guid? LeadId { get; set; }

        public string PartnerCode { get; set; }


        public void FillStatuses()
        {
            OnlineTaskStatus statusenum;
            if (Enum.TryParse(Status, out statusenum))
            {
                Status = statusenum.GetDisplayName();
            }

            OnlineTaskType typeEnum;
            if (Enum.TryParse(Type, out typeEnum))
            {
                Type = typeEnum.GetDisplayName();
            }
        }
    }
}
