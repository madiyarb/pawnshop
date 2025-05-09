using Microsoft.AspNetCore.Mvc;
using Pawnshop.Web.Models.Page;
using System;

namespace Pawnshop.Web.Models.Comment
{
    public class CommentListQueryView : PageSettingFromQuery
    {
        [FromQuery(Name = "appId")]
        public Guid? ApplicationOnlineId { get; set; }

        [FromQuery(Name = "clientId")]
        public int? ClientId { get; set; }
    }
}
