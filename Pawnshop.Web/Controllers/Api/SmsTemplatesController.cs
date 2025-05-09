using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Sms;
using Pawnshop.Web.Models.Sms;
using System.Collections.Generic;
using System.Linq;
using System;
using Pawnshop.Core;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/sms/templates")]
    [ApiController]
    [Authorize]
    public class SmsTemplatesController : ControllerBase
    {
        private readonly SmsMessageAttributeRepository _smsMessageAttributeRepository;
        private readonly SmsMessageTypeRepository _smsMessageTypeRepository;
        private readonly SmsTemplateRepository _smsTemplateRepository;

        public SmsTemplatesController(
            SmsMessageAttributeRepository smsMessageAttributeRepository,
            SmsMessageTypeRepository smsMessageTypeRepository,
            SmsTemplateRepository smsTemplateRepository)
        {
            _smsMessageAttributeRepository = smsMessageAttributeRepository;
            _smsMessageTypeRepository = smsMessageTypeRepository;
            _smsTemplateRepository = smsTemplateRepository;
        }

        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpPost]
        public ActionResult<SmsTemplateView> Create([FromBody] SmsTemplateCreateBinding binding)
        {
            var entity = new SmsTemplate
            {
                CreateDate = DateTime.Now,
                SmsMessageTypeId = binding.SmsMessageTypeId,
                ManualSendRoleId = binding.ManualSendRoleId > 0 ? binding.ManualSendRoleId : null,
                MessageTemplate = binding.MessageTemplate,
                Title = binding.Title
            };

            _smsTemplateRepository.Insert(entity);

            entity = _smsTemplateRepository.Get(entity.Id);

            return Ok(ToSmsTemplateView(entity));
        }

        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            var entity = _smsTemplateRepository.Get(id);

            if (entity == null)
                return BadRequest("Шаблон не найден!");

            _smsTemplateRepository.Delete(id);

            return Ok();
        }

        [HttpGet("{id}")]
        public ActionResult<SmsTemplateView> Get([FromRoute] int id)
        {
            var entity = _smsTemplateRepository.Get(id);
            return Ok(ToSmsTemplateView(entity));
        }

        [HttpGet("attributes")]
        public ActionResult<IList<SmsMessageAttributeView>> GetAttributes([FromQuery] string entityType)
        {
            var entities = _smsMessageAttributeRepository.List(null, new { EntityType = entityType });
            var list = entities.Select(x =>
                new SmsMessageAttributeView
                {
                    Attribute = x.Attribute,
                    CreateDate = x.CreateDate,
                    Id = x.Id,
                    SmsMessageTypeId = x.SmsMessageTypeId,
                    SmsMessageTypeName = x.SmsMessageType?.Title,
                    Title = x.Title,
                });

            return Ok(list);
        }

        [HttpGet("list")]
        public ActionResult<SmsTemplateListView> GetList([FromQuery] SmsTemplateListQuery query)
        {
            var entities = _smsTemplateRepository.List(null, query);

            var response = new SmsTemplateListView();
            response.Count = _smsTemplateRepository.Count(null, query);
            response.List = entities.Select(x => ToSmsTemplateView(x))
                .ToList();

            return Ok(response);
        }

        [HttpGet("smsmessagetype")]
        public ActionResult<IList<SmsMessageType>> GetSmsMessageType()
        {
            return Ok(_smsMessageTypeRepository.List(null));
        }

        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpPut("{id}")]
        public ActionResult<SmsTemplateView> Update([FromRoute] int id, [FromBody] SmsTemplateUpdateBinding binding)
        {
            var entity = _smsTemplateRepository.Get(id);

            if (entity == null)
                return BadRequest("Шаблон не найден!");

            if (binding.SmsMessageTypeId.HasValue)
                entity.SmsMessageTypeId = binding.SmsMessageTypeId;

            if (!string.IsNullOrEmpty(binding.Title))
                entity.Title = binding.Title;

            if (binding.ManualSendRoleId.HasValue && binding.ManualSendRoleId > 0)
                entity.ManualSendRoleId = binding.ManualSendRoleId;

            if (!string.IsNullOrEmpty(binding.MessageTemplate))
                entity.MessageTemplate = binding.MessageTemplate;

            _smsTemplateRepository.Update(entity);

            entity = _smsTemplateRepository.Get(entity.Id);

            return Ok(entity);
        }


        private SmsTemplateView ToSmsTemplateView(SmsTemplate smsTemplate)
        {
            return new SmsTemplateView
            {
                CreateDate = smsTemplate.CreateDate,
                Id = smsTemplate.Id,
                ManualSendRoleId = smsTemplate.ManualSendRoleId,
                ManualSendRoleName = smsTemplate.Role?.Name,
                MessageTemplate = smsTemplate.MessageTemplate,
                SmsMessageTypeId = smsTemplate.SmsMessageTypeId,
                SmsMessageTypeName = smsTemplate.SmsMessageType?.Title,
                Title = smsTemplate.Title,
                UpdateDate = smsTemplate.UpdateDate,
            };
        }
    }
}
