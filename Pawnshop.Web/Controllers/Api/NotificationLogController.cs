using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Models.Notification;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.NotificationView)]
    public class NotificationLogController : Controller
    {
        private readonly NotificationLogRepository _repository;

        public NotificationLogController(NotificationLogRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public List<NotificationLog> List([FromBody] ListQueryModel<NotificationLogListQueryModel> listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            if (listQuery.Model == null) throw new ArgumentNullException(nameof(listQuery.Model));

            return _repository.List(listQuery, listQuery.Model);
        }
    }
}
