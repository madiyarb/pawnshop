using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Models.InnerNotification;

namespace Pawnshop.Web.Controllers.Api
{
    public class InnerNotificationController : Controller
    {
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        private readonly EmailSender _emailSender;
        private readonly SmsSender _smsSender;

        public InnerNotificationController(InnerNotificationRepository innerNotificationRepository,
            ISessionContext sessionContext, BranchContext branchContext,
            EmailSender emailSender, SmsSender smsSender)
        {
            _innerNotificationRepository = innerNotificationRepository;

            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _emailSender = emailSender;
            _smsSender = smsSender;
        }

        [HttpPost]
        public ListModel<InnerNotification> List([FromBody] ListQueryModel<InnerNotificationListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<InnerNotificationListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new InnerNotificationListQueryModel();
            listQuery.Model.BranchId = _branchContext.Branch.Id;
            listQuery.Model.UserId = _sessionContext.UserId;

            return new ListModel<InnerNotification>
            {
                List = _innerNotificationRepository.List(listQuery, listQuery.Model),
                Count = _innerNotificationRepository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public InnerNotification Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _innerNotificationRepository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost]
        [Event(EventCode.NotificationSaved, EventMode = EventMode.Response, EntityType = EntityType.InnerNotification)]
        public InnerNotification Save([FromBody] InnerNotification model)
        {
            
            if (model.Id > 0)
            {
                _innerNotificationRepository.Update(model);
            }
            else
            {
                _innerNotificationRepository.Insert(model);
            }

            return model;
        }

        [HttpPost]
        [Event(EventCode.InnerNotificationDeleted, EventMode = EventMode.Request, EntityType = EntityType.InnerNotification)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _innerNotificationRepository.Get(id);
            if (model == null) throw new InvalidOperationException();

            _innerNotificationRepository.Delete(id);
            return Ok();
        }

        [HttpPost]
        [Event(EventCode.InnerNotificationRead, EventMode = EventMode.Request, EntityType = EntityType.InnerNotification)]
        public IActionResult MarkAsRead([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _innerNotificationRepository.Get(id);
            if (model == null) throw new InvalidOperationException();

            _innerNotificationRepository.MarkAsRead(id);
            return Ok();
        }

        [HttpPost]
        [Event(EventCode.InnerNotificationDone, EventMode = EventMode.Request, EntityType = EntityType.InnerNotification)]
        public IActionResult MarkAsDone([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _innerNotificationRepository.Get(id);
            if (model == null) throw new InvalidOperationException();

            _innerNotificationRepository.MarkAsDone(id);
            return Ok();
        }
    }
}