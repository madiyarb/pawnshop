using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.ManualUpdate
{
    public class ManualUpdateModel : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public int CategoryId { get; set; }
        public string SelectQuery { get; set; }
        public string SelectResult { get; set; }
        public string UpdateQuery { get; set; }
        public string UpdateResult { get; set; }
        public ManualUpdateStatus Status { get; set; }
        public string Message { get; set; }
    }
}