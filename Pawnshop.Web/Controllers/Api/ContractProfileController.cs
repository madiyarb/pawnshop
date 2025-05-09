
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services;
using System;
using Pawnshop.Services.Models.List;
using Pawnshop.Core.Queries;
using Pawnshop.Web.Engine;
using Microsoft.AspNetCore.Authorization;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts;


namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractView)]
    public class ContractProfileController : Controller
    {
        private readonly IBaseService<ContractProfile> _service;
        private readonly ISessionContext _sessionContext;

        public ContractProfileController(IBaseService<ContractProfile> service, ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost, Authorize(Permissions.ContractManage)]
        public ContractProfile Save([FromBody] ContractProfile model)
        {
            ModelState.Validate();

            model.AuthorId = _sessionContext.IsInitialized ? _sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY;
            model.CreateDate = DateTime.Now;

            return _service.Save(model);
        }

        [HttpPost]
        public ContractProfile Card([FromBody] int contractId)
        {
            if (contractId <= 0) throw new ArgumentOutOfRangeException(nameof(contractId));

            var contractProfile = _service.Find(new { ContractId = contractId });

            return contractProfile;
        }
    }
}