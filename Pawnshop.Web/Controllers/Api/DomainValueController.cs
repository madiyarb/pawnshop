using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Services;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Domains;
using Pawnshop.Web.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/domains/{domainCode}/values"), Authorize]
    public class DomainValueController : Controller
    {
        private readonly IDomainService _domainService;
        private readonly IMemoryCache _memoryCache;
        public DomainValueController(IDomainService domainService,
            IMemoryCache memoryCache)
        {
            _domainService = domainService;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Получает список всех значений доменов
        /// </summary>
        /// <param name="domainCode">Код домена</param>
        /// <returns></returns>
        [HttpPost("list/all"), ProducesResponseType(typeof(List<DomainValueDto>), 200)]
        public IActionResult List([FromRoute] string domainCode)
        {   
            ModelState.Validate();
            if (!_memoryCache.TryGetValue(domainCode, out List<DomainValue> domainValues))
            {
                domainValues = _domainService.GetDomainValues(domainCode);
                _memoryCache.Set(domainCode, domainValues,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }

            return Ok(domainValues.Select(dv => new DomainValueDto
            {
                Id = dv.Id,
                AdditionalData = dv.AdditionalData != null ? JsonConvert.DeserializeObject(dv.AdditionalData) : null,
                IsActive = dv.IsActive,
                NameAlt = dv.NameAlt,
                Code = dv.Code,
                Name = dv.Name
            }));
        }

        /// <summary>
        /// Получает список значений доменов
        /// </summary>
        /// <param name="domainCode">Код домена</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("list"), ProducesResponseType(typeof(ListModel<DomainValueDto>), 200)]
        public IActionResult List([FromRoute] string domainCode, [FromBody] ListQuery request)
        {
            ModelState.Validate();
            (List<DomainValue> domainValues, int count) = _domainService.GetDomainValues(domainCode, request);
            return Ok(new ListModel<DomainValueDto>
            {
                List = domainValues.Select(dv => new DomainValueDto
                {
                    Id = dv.Id,
                    AdditionalData = dv.AdditionalData != null ? JsonConvert.DeserializeObject(dv.AdditionalData) : null,
                    IsActive = dv.IsActive,
                    NameAlt = dv.NameAlt,
                    Code = dv.Code,
                    Name = dv.Name
                }).ToList(),
                Count = count
            });
        }

        /// <summary>
        /// Получает список значений доменов
        /// </summary>
        /// <param name="code">Код домена</param>
        /// <param name="domainvaluecode">Код значения домена</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("get/{domainValueId:int}"), ProducesResponseType(typeof(DomainValueDto), 200)]
        public IActionResult Get([FromRoute] string domainCode, [FromRoute] int domainValueId)
        {
            DomainValue domainValue = _domainService.GetDomainValue(domainCode, domainValueId);
            return Ok(new DomainValueDto
            {
                Id = domainValue.Id,
                AdditionalData = domainValue.AdditionalData != null ? JsonConvert.DeserializeObject(domainValue.AdditionalData) : null,
                IsActive = domainValue.IsActive,
                NameAlt = domainValue.NameAlt,
                Code = domainValue.Code,
                Name = domainValue.Name
            });
        }

        /// <summary>
        /// Сохраняет значение домена
        /// </summary>
        /// <param name="domainCode">Код домена</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("save"), Authorize(Permissions.DomainManage)]
        public IActionResult SaveDomainValue([FromRoute] string domainCode, [FromBody] DomainValueDto request)
        {
            ModelState.Validate();
            DomainValue domainValue = _domainService.SaveDomainValue(domainCode, request);
            return Ok(new DomainValueDto
            {
                Id = domainValue.Id,
                AdditionalData = domainValue.AdditionalData != null ? JsonConvert.DeserializeObject(domainValue.AdditionalData) : null,
                IsActive = domainValue.IsActive,
                NameAlt = domainValue.NameAlt,
                Code = domainValue.Code,
                Name = domainValue.Name
            });
        }

        /// <summary>
        /// Удаляет домен(soft delete)
        /// </summary>
        /// <param name="code">Код домена</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("delete/{domainValueId:int}"), Authorize(Permissions.DomainManage)]
        public IActionResult Delete([FromRoute] string domainCode, [FromRoute] int domainValueId)
        {
            _domainService.DeleteDomainValue(domainCode, domainValueId);
            return Ok();
        }

        [HttpPost("list/manualBuyoutReasons"), ProducesResponseType(typeof(List<DomainValueDto>), 200)]
        public IActionResult ListManualBuyoutReasons([FromRoute] string domainCode)
        {
            ModelState.Validate();
            List<DomainValue> domainValues = _domainService.GetDomainValuesForManualBuyoutReasons();
            return Ok(domainValues.Select(dv => new DomainValueDto
            {
                Id = dv.Id,
                AdditionalData = dv.AdditionalData != null ? JsonConvert.DeserializeObject(dv.AdditionalData) : null,
                IsActive = dv.IsActive,
                NameAlt = dv.NameAlt,
                Code = dv.Code,
                Name = dv.Name
            }));
        }
    }
}
