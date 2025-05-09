using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;

namespace Pawnshop.Web.Engine.Middleware
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : Attribute, IActionFilter
    {
        public EventCode Code { get; }

        public EventMode EventMode { get; set; }

        public EntityType EntityType { get; set; }

        public bool IncludeFails { get; set; }

        public EventAttribute(EventCode code)
        {
            Code = code;
            EventMode = EventMode.None;
            EntityType = EntityType.None;
            IncludeFails = true;
        }

        private string _reqData = null;
        private int? _entityId = null;
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _reqData = null;
            if (EventMode == EventMode.Request || EventMode == EventMode.All)
            {
                _reqData = Format(context.ActionArguments.Values.ToArray());
                if (EntityType != EntityType.None)
                {
                    _entityId = LookForEntity(context.ActionArguments.Values.ToArray());
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (IncludeFails && context.Exception != null)
            {
                Log(context.HttpContext, Code, EventStatus.Failed, EntityType, _entityId, _reqData, JsonConvert.SerializeObject(context.Exception));
                return;
            }

            string resData = null;
            if ((EventMode == EventMode.Response || EventMode == EventMode.All) && context.Result is ObjectResult)
            {
                var result = (ObjectResult)context.Result;
                resData = Format(result.Value);
                if (EntityType != EntityType.None)
                {
                    _entityId = LookForEntity(result.Value);
                }
            }
            try
            {
                Log(context.HttpContext, Code, EventStatus.Success, EntityType, _entityId, _reqData, resData);
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
        }

        private void Log(HttpContext context, EventCode code, EventStatus status, EntityType entityType, int? entityId, string requestData, string responseData)
        {
            var scope = (ILifetimeScope)context.RequestServices.GetService(typeof(ILifetimeScope));
            var eventLog = scope.Resolve<Audit.EventLog>();

            eventLog.Log(code, status,
                entityType == EntityType.None ? (EntityType?) null : entityType,
                entityId, requestData, responseData);
        }

        private string Format(params object[] args)
        {
            var stringBuilder = new StringBuilder();

            foreach (var arg in args)
            {
                if (arg == null) continue;

                var loggable = arg as ILoggable;
                var obj = loggable != null ? loggable.Format() : arg;
                var formatted = JsonConvert.SerializeObject(obj, Formatting.None);

                if (!string.IsNullOrWhiteSpace(formatted))
                {
                    stringBuilder.AppendLine(formatted);
                }
            }

            return stringBuilder.ToString();
        }

        private int? LookForEntity(params object[] args)
        {
            int? entityId = null;
            foreach (var arg in args)
            {
                if (arg is ILoggableToEntity)
                {
                    entityId = ((ILoggableToEntity) arg).GetLinkedEntityId();
                }
                else if (arg is IEntity)
                {
                    entityId = ((IEntity) arg).Id;
                }
                else if (arg is int)
                {
                    entityId = (int) arg;
                }
                if (entityId.HasValue) break;
            }
            return entityId;
        }
    }

    public enum EventMode
    {
        None,
        Request,
        Response,
        All
    }
}