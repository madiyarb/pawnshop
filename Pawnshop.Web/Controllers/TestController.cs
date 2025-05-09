using Microsoft.AspNetCore.Mvc;
using Pawnshop.Web.Engine.Jobs.AccountingJobs;
using Pawnshop.Web.Engine.Jobs;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Services.Collection;
using Pawnshop.Services.Kato;
using Pawnshop.Services.TasLabBankrupt;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Web.Controllers
{
    public class TestController : Controller
    {
        DelayNotificationJob _notification;
        public TestController(DelayNotificationJob notification) 
        {
            _notification = notification;
        }

        [HttpGet("/api/exec")]
        public async Task<IActionResult> executePenaltyAccrualJob()
        {
            _notification.Execute();
            return Ok();
        }

        //[HttpGet("/api/testCrmUploadContactJob")]
        //public async Task<IActionResult> TestCrmUploadContactJob([FromServices] CrmUploadContactJob job)
        //{
        //    await job.Execute();
        //    return Ok();
        //}

        //[HttpGet("/api/testCrmUploadJob")]
        //public async Task<IActionResult> TestCrmUploadJob()
        //{
        //    return Ok(await _crmUploadJob.Execute());
        //}

        //[HttpGet("/api/bankruptTest")]
        //public async Task<IActionResult> GetBankruptInfo([FromQuery] string iin, [FromServices] ITasLabBankruptInfoService service,
        //    CancellationToken cancellationToken)
        //{
        //    var a = await service.IsClientBankruptFromDatabase(iin, cancellationToken);
        //    var b = await service.IsClientBankruptOnline(iin, cancellationToken);

        //    if (a || b)
        //        return BadRequest();
        //    return Ok();
        //}

        //[HttpGet("/api/test/InsurancePoliciesCancelJob/execute")]
        //public IActionResult InsurancePoliciesCancelJobStarter()
        //{
        //    _insurancePoliciesCancelJob.Execute();

        //    return Ok();
        //}

        //[HttpGet("/api/test/{id}/creditlinejob")]
        //public async Task<IActionResult> CreditlineJob([FromServices] UsePrepaymentForCreditLineForMonthlyPaymentJob job, int id)
        //{
        //    job.UsePrepaymentForCreditLine(id, 4);
        //    return Ok();
        //}

        //[HttpGet("/api/onlinePayment")]
        //public async Task<IActionResult> OnlinePaymentJob([FromServices] OnlinePaymentJob job)
        //{
        //    job.Execute();
        //    return Ok();
        //}

        //[HttpGet("/api/test/AccountIntegrationJob/execute")]
        //public IActionResult AccountIntegrationManualStarter([FromServices] AccountantIntegrationJob accountantIntegrationJob)
        //{
        //    accountantIntegrationJob.Execute();
        //    return Ok();
        //}

        //[HttpGet("api/test/CancelApplicationOnlineJob/execute")]
        //public IActionResult CancelApplicationOnline([FromServices] CancelApplicationOnlineJob cancelApplicationOnlineJob)
        //{
        //    cancelApplicationOnlineJob.Execute();
        //    return Ok();
        //}

        //[HttpPost("api/test/InterestAccrualJob")]
        //public IActionResult InterestAccrualJobManual( 
        //    [FromBody] DateTime date)
        //{
        //    _interestAccrualJob.EnqueueOnSomeDate(date);
        //    return Ok();
        //}

        //[HttpPost("api/test/TakeAwayToDelayJob")]
        //public IActionResult TakeAwayToDelayJobManual([FromServices] TakeAwayToDelayJob takeAwayToDelayJob,
        //    [FromBody] DateTime date)
        //{
        //    takeAwayToDelayJob.ExecuteOnDate(date);
        //    return Ok();
        //}
        //[HttpPost("api/test/PenaltyRateDecreaseJob")]
        //public IActionResult PenaltyRateDecreaseJobManual([FromServices] PenaltyRateDecreaseJob job,
        //    [FromBody] DateTime date)
        //{
        //    job.ExecuteOnDate(date);
        //    return Ok();
        //}

        //[HttpPost("api/test/PenaltyLimitAccrualJob")]
        //public IActionResult PenaltyLimitAccrualJobManual([FromServices] PenaltyLimitAccrualJob job,
        //    [FromBody] DateTime date)
        //{
        //    job.ExecuteOnDate(date);
        //    return Ok();
        //}

        //[HttpPost("api/test/PenaltyAccrualJob")]
        //public IActionResult PenaltyAccrualJobJobManual([FromServices] PenaltyAccrualJob job,
        //    [FromBody] DateTime date)
        //{
        //    job.ExecuteOnAnyDate(date);
        //    return Ok();
        //}

        //[HttpPost("api/test/InterestAccrualOnOverdueDebtJob")]
        //public IActionResult InterestAccrualOnOverdueDebtJob([FromServices] InterestAccrualOnOverdueDebtJob job,
        //    [FromBody] DateTime date)
        //{
        //    job.ExecuteOnAnyDate(date);
        //    return Ok();
        //}

        //[HttpPost("api/test/CollectionJob")]
        //public IActionResult CollectionJob([FromServices] CollectionJob job)
        //{
        //    job.Execute();
        //    return Ok();
        //}
    }
}