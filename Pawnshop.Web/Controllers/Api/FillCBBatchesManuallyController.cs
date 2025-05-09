using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Pawnshop.Data.Models.FillCBBatchesManually;
using Pawnshop.Services.CBBatches;
using Pawnshop.Core.Exceptions;
using Newtonsoft.Json;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Web.Engine.Jobs;
using Hangfire;
using System.Threading;
using System.Text.RegularExpressions;
using Pawnshop.Data.Models.CreditBureaus;
using Newtonsoft.Json.Linq;
using Pawnshop.Core;

namespace Pawnshop.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class FillCBBatchesManuallyController : Controller
    {
        private readonly CBBatchesService _cbBatchesService;
        private readonly EnviromentAccessOptions _options;
        private readonly ISessionContext _sessionContext;

        public FillCBBatchesManuallyController(IOptions<EnviromentAccessOptions> options, CBBatchesService cbBatchesDataFromFile, ISessionContext sessionContext)
        {
            _cbBatchesService = cbBatchesDataFromFile;
            _options = options.Value;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        [Route("FillBatches")]
        public async Task<IActionResult> FillBatches([FromForm] FillCBBatchesManuallyRequest req)
        {

            if (!_options.CBUpload) throw new PawnshopApplicationException("нельзя создать батчи!");

            if (req == null) throw new PawnshopApplicationException("Введите данные");
            if (req.date.Date > DateTime.Today.Date) throw new PawnshopApplicationException("Введена неверная дата");

            var cbs = JsonConvert.DeserializeObject<int[]>(req.cbs);

            if (cbs.Length == 0 || cbs.Length > 2) throw new PawnshopApplicationException("КБ не указано");

            if (req.contractIds != null && req.file == null)
                req.contractIds = Regex.Replace(req.contractIds, @"[^0-9,]+", "");

            if(req.file == null && req.contractIds == null)
                throw new PawnshopApplicationException("Загрузите файл или введите Id контрактов вручную");
            else if(req.file != null && req.contractIds != null)
                throw new PawnshopApplicationException("Можно отправить только файл или только строку введеную вручную");

            if(req.isDaily == null || req.isDaily.Length == 0)
                throw new PawnshopApplicationException("Укажите вид батча, ежедневный или еженедельный");

            int userId = _sessionContext.UserId;
            List<int> batchIds = _cbBatchesService.CreateCBBatches(req, userId);
            if(batchIds == null || batchIds.Count == 0) throw new PawnshopApplicationException("батчи не были созданы, проверьте правильность Id");

            // Job запускается здесь, в сервисах нельзя, иначе это приведет циклической зависимоти
            BackgroundJob.Enqueue<CBBatchDataFulfillJob>(x => x.Execute());

            List<string> fileNames = new List<string>();

            // если батчи большие 2000+ контрактов, проект застынет на долго
            while (fileNames.Count != batchIds.Count)
            {
                foreach (int batchId in batchIds)
                {
                    string filename = _cbBatchesService.GetBatchById(batchId).FileName;
                    if(filename != null && filename != "" && filename.Length > 0 && !fileNames.Contains(filename))
                        fileNames.Add(filename);
                }
                if(fileNames.Count == 0)
                    Thread.Sleep(5000);
            }

            if (fileNames == null || fileNames.Count == 0) throw new PawnshopApplicationException("батчи создались, но не заполнились");

            JArray records = new JArray();

            for (int i = 0; i < batchIds.Count; i++)
            {
                JObject record = new JObject();
                record["batchId"] = batchIds[i];
                record["cbId"] = (_cbBatchesService.GetBatchById(batchIds[i]).CBId == CBType.FCB ? "ПКБ" : "ГКБ");
                record["fileName"] = fileNames[i];
                records.Add(record);
            }

            return Ok(records.ToString());
        }
    }
}