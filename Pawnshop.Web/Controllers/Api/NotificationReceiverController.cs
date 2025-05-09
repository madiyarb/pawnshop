using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Models.Notification;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.NotificationView)]
    public class NotificationReceiverController : Controller
    {
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationLogRepository _notificationLogRepository;
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;

        public NotificationReceiverController(NotificationReceiverRepository notificationReceiverRepository,
            NotificationLogRepository notificationLogRepository,
            ISessionContext sessionContext, BranchContext branchContext)
        {
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationLogRepository = notificationLogRepository;

            _sessionContext = sessionContext;
            _branchContext = branchContext;
        }

        [HttpPost]
        public ListModel<NotificationReceiver> List([FromBody] ListQueryModel<NotificationReceiverListQueryModel> listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            if (listQuery.Model == null) throw new ArgumentNullException(nameof(listQuery.Model));

            return new ListModel<NotificationReceiver>
            {
                List = _notificationReceiverRepository.List(listQuery, listQuery.Model),
                Count = _notificationReceiverRepository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost, Authorize(Permissions.NotificationManage)]
        [Event(EventCode.NotificationSaved, EventMode = EventMode.Response, EntityType = EntityType.Notification)]
        public NotificationReceiver Save([FromBody] NotificationReceiver model)
        {
            ModelState.Clear();
            TryValidateModel(model);
            ModelState.Validate();

            if (model.Id > 0)
            {
                _notificationReceiverRepository.Update(model);
            }
            else
            {
                _notificationReceiverRepository.Insert(model);
            }

            return model;
        }

        [HttpPost, Authorize(Permissions.NotificationManage)]
        [Event(EventCode.NotificationDeleted, EventMode = EventMode.Request, EntityType = EntityType.Notification)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _notificationReceiverRepository.Get(id);
            if (model == null) throw new InvalidOperationException();
            if (model.Status > NotificationStatus.Draft) throw new PawnshopApplicationException("Уведомление уже установлено для отправки");

            _notificationReceiverRepository.Delete(id);
            return Ok();
        }

        [HttpPost]
        public IActionResult Select([FromBody] NotificationClientListQueryModel listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            listQuery.BranchId = _branchContext.Branch.Id;

            _notificationReceiverRepository.Select(new ListQuery(), listQuery);

            return Ok();
        }
    }
}
