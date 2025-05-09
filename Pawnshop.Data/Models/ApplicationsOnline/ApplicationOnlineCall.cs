using Pawnshop.Data.Models.Calls;
using System;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public class ApplicationOnlineCall
    {
        public int Id { get; set; }

        public Guid ApplicationOnlineId { get; set; }

        public ApplicationOnline ApplicationOnline { get; set; }

        public int CallId { get; set; }

        public Call Call { get; set; }
    }
}
