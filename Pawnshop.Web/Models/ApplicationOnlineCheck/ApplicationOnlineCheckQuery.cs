using Microsoft.AspNetCore.Mvc;
using System;

namespace Pawnshop.Web.Models.ApplicationOnlineCheck
{
    public class ApplicationOnlineCheckQuery
    {
        [FromQuery]
        public Guid ApplicationOnlineId { get; set; }
    }
}
