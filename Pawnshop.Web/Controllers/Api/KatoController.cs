using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models.Kato;
using Pawnshop.Services.Kato;

namespace Pawnshop.Web.Controllers.Api
{
    public class KatoController : Controller
    {
        private readonly IKatoService _katoService;
        public KatoController(IKatoService katoService) 
        {
            _katoService = katoService;
        }

        [HttpPost("/api/kato/runKatoService")]
        public IActionResult RunKatoService([FromBody] KatoModel model)
        {
            _katoService.StartWork(model.FileUrl);
            return Ok();
        }
    }
}
