using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models._1c;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Middleware;

namespace Pawnshop.Data.Access
{
    [AllowAnonymous]
    public class PayOperationOuterController : Controller
    {
        private readonly PayOperationRepository _payOperationRepository;
        private readonly PayOperationQueryRepository _payOperationQueryRepository;

        public PayOperationOuterController(PayOperationRepository payOperationRepository,
            PayOperationQueryRepository payOperationQueryRepository)
        {
            _payOperationRepository = payOperationRepository;
            _payOperationQueryRepository = payOperationQueryRepository;
        }

        [HttpGet,AllowAnonymous]
        [Event(EventCode.AccountantStatusCheck, EventMode = EventMode.Response)]
        public IActionResult Done(int id, string token)
        {
            if(!String.Equals(token, "a9d473f5c54b")) return Unauthorized();
            if (id <= 0) return BadRequest();

            try
            {
                var operation = _payOperationRepository.Get(id);

                if (operation.Status == Models.PayOperations.PayOperationStatus.Checked)
                {
                    PayOperationQuery query = new PayOperationQuery {
                        AuthorId = 1,
                        CreateDate = DateTime.Now,
                        OperationId = operation.Id,
                        Status = QueryStatus.Queued,
                        QueryType = QueryType.Check
                    };
                    using(var transaction = _payOperationQueryRepository.BeginTransaction())
                    {
                        _payOperationQueryRepository.Insert(query);

                        transaction.Commit();
                    }
                }
                return Ok();
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }
    }
}