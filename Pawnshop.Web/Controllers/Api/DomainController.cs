using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
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
    [Route("api/domains"), Authorize]
    public class DomainController : Controller
    {
        private readonly IDomainService _domainService;
        public DomainController(IDomainService domainService)
        {
            _domainService = domainService;
        }

        /// <summary>
        /// Получает список всех доменов
        /// </summary>
        /// <returns></returns>
        [HttpPost("list/all"), ProducesResponseType(typeof(List<DomainDto>), 200)]
        public IActionResult GetDomains()
        {
            ModelState.Validate();
            List<Domain> domains = _domainService.GetDomains();
            return Ok(domains.Select(d => new DomainDto
            {
                Code = d.Code,
                NameAlt = d.NameAlt,
                Name = d.Name
            }));
        }

        /// <summary>
        /// Получает список доменов
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("list"), ProducesResponseType(typeof(ListModel<DomainDto>), 200)]
        public IActionResult GetDomains([FromBody] GetDomainsRequest request)
        {
            ModelState.Validate();
            (List<Domain> domains, int count) = _domainService.GetDomains(request);
            return Ok(new ListModel<DomainDto>
            {
                List = domains.Select(d => new DomainDto
                {
                    Code = d.Code,
                    NameAlt = d.NameAlt,
                    Name = d.Name
                }).ToList(),
                Count = count
            });
        }

        /// <summary>
        /// Получает домен
        /// </summary>
        /// <param name="code">Код домена</param>
        /// <returns></returns>
        [HttpPost("get/{code}"), ProducesResponseType(typeof(DomainDto), 200)]
        public IActionResult GetDomain([FromRoute] string code)
        {
            Domain domain = _domainService.GetDomain(code);
            return Ok(new DomainDto
            {
                Code = domain.Code,
                NameAlt = domain.NameAlt,
                Name = domain.Name,
            });
        }
    }
}
