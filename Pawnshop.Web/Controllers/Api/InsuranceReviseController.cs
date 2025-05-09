using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Insurance;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Services.Storage;
using Pawnshop.Web.Models.Insurance;
using Pawnshop.Web.Models.List;
using System;
using System.Dynamic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using Pawnshop.Data.Models.FillCBBatchesManually;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.CashOrders;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.StaticFiles;

namespace Pawnshop.Web.Controllers.Api
{
    public class InsuranceReviseController : Controller
    {
        private readonly IInsuranceReviseService _insuranceReviseService;
        private readonly InsuranceReviseRowsExcelBuilder _excelBuilder;
        private readonly IStorage _storage;
        private readonly InsuranceReviseRepository _insuranceReviseRepository;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;

        public InsuranceReviseController(
            IInsuranceReviseService insuranceReviseService, 
            InsuranceReviseRowsExcelBuilder excelBuilder, 
            IStorage storage, 
            InsuranceReviseRepository insuranceReviseRepository, 
            BranchContext branchContext, 
            ISessionContext sessionContext)
        {
            _insuranceReviseService = insuranceReviseService;
            _excelBuilder = excelBuilder;
            _storage = storage;
            _insuranceReviseRepository = insuranceReviseRepository;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        [Authorize(Permissions.InsuranceRevise)]
        public ListModel<InsuranceRevise> List([FromBody] ListQueryModel<InsuranceReviseListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<InsuranceReviseListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new InsuranceReviseListQueryModel();

            if (listQuery.Model.EndDate.HasValue) listQuery.Model.EndDate = listQuery.Model.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            return new ListModel<InsuranceRevise>
            {
                List = _insuranceReviseRepository.List(listQuery, listQuery.Model),
                Count = _insuranceReviseRepository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        [Authorize(Permissions.InsuranceRevise)]
        public InsuranceRevise Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _insuranceReviseRepository.Get(id);
            if (model == null) throw new PawnshopApplicationException("Такой сверки не существует в Финкоре");

            return model;
        }

        [HttpPost]
        [Authorize(Permissions.InsuranceRevise)]
        public async Task<IActionResult> Export([FromBody] List<InsuranceReviseRow> insuranceReviseRows)
        {
            using (var stream = _excelBuilder.Build(insuranceReviseRows))
            {
                var fileName = await _storage.Save(stream, ContainerName.Temp, "export.xlsx");
                string contentType;
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);

                var fileRow = new FileRow
                {
                    CreateDate = DateTime.Now,
                    ContentType = contentType ?? "application/octet-stream",
                    FileName = fileName,
                    FilePath = fileName
                };
                return Ok(fileRow);
            }
        }

        [HttpPost]
        [Authorize(Permissions.InsuranceRevise)]
        public InsuranceRevise Save([FromForm] InsuranceReviseRequest req)
        {
            if (req == null) throw new PawnshopApplicationException("Данные отсутствуют");
            if (req.beginDate == null) throw new PawnshopApplicationException("Введите начальную дату сверки");
            if (req.endDate == null) throw new PawnshopApplicationException("Введите конечную дату сверки");
            if (req.file == null) throw new PawnshopApplicationException("Файл не загрузите");
            if (req.insuranceCompanyId == 0) throw new PawnshopApplicationException("Страховая компания не выбрана");

            InsuranceRevise revise = _insuranceReviseService.CreateInsuranceRevise(req);
            if (revise == null) throw new PawnshopApplicationException("Сверка не были созданы, проверьте Файл");

            return revise;
        }
    }
}
