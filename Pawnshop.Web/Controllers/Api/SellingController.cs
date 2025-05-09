using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Sellings;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.Sellings;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Calculation;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Services.Storage;
using Pawnshop.Web.Models.Contract;
using Pawnshop.Services.Models.List;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.SellingView)]
    public class SellingController : Controller
    {
        private readonly SellingsExcelBuilder _excelBuilder;
        private readonly IStorage _storage;
        private readonly BranchContext _branchContext;
        private readonly IContractActionSellingService _contractSellingService;
        private readonly GroupRepository _groupRepository;
        private readonly Group _mainBranch;

        public SellingController(SellingsExcelBuilder excelBuilder, 
                                 IStorage storage,
                                 BranchContext branchContext,
                                 IContractActionSellingService contractSellingService,
                                 GroupRepository groupRepository)
        {
            _excelBuilder = excelBuilder;
            _storage = storage;
            _branchContext = branchContext;
            _contractSellingService = contractSellingService;
            _groupRepository = groupRepository;
            _mainBranch = groupRepository.Find(new { Name = Constants.BKS });
        }

        [HttpPost, Authorize(Permissions.SellingView)]
        public ListModel<Selling> List([FromBody] ListQueryModel<SellingListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<SellingListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new SellingListQueryModel();

            listQuery.Model.CurrentBranchName = _branchContext.Branch.Name;
            listQuery.Model.CurrentBranchId = _branchContext.Branch.Id;

            _contractSellingService.enrichQuery(listQuery);

            return _contractSellingService.List(listQuery);
        }

        [HttpPost, Authorize(Permissions.SellingView)]
        public IActionResult Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            return Ok(_contractSellingService.GetSelling(id));
        }

        [HttpPost, Authorize(Permissions.SellingView)]
        public IActionResult GetSellingDuty([FromBody] SellingDuty sellingDuty)
        {
            ModelState.Validate();
            return Ok(_contractSellingService.GetSellingDuty(sellingDuty));
        }

        [HttpPost, Authorize(Permissions.SellingManage)]
        [Event(EventCode.SellingSell, EventMode = EventMode.Response, EntityType = EntityType.Selling)]
        public IActionResult Sell([FromBody] Selling selling)
        {
            if (_mainBranch == null)
                throw new PawnshopApplicationException($"Филиал {Constants.BKS} не найден");

            if (selling == null)
                throw new ArgumentOutOfRangeException(nameof(selling));
            return Ok(_contractSellingService.Sell(selling, _mainBranch.Id));
        }

        [HttpPost, Authorize(Permissions.SellingManage)]
        [Event(EventCode.SellingSellCancel, EventMode = EventMode.Request, EntityType = EntityType.Selling)]
        public IActionResult Cancel([FromBody] int id)
        {
            if (_mainBranch == null)
                throw new PawnshopApplicationException($"Филиал {Constants.BKS} не найден");

            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            _contractSellingService.Cancel(id, _mainBranch.Id);
            return Ok();
        }

        [HttpPost, Authorize(Permissions.SellingManage)]
        [Event(EventCode.SellingDeleted, EventMode = EventMode.Request, EntityType = EntityType.Selling)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            _contractSellingService.Delete(id);
            return Ok();
        }

        [HttpPost, Authorize(Permissions.SellingManage)]
        [Event(EventCode.SellingSaved, EventMode = EventMode.Response, EntityType = EntityType.Selling)]
        public IActionResult Save([FromBody] Selling selling)
        {
            ModelState.Clear();
            TryValidateModel(selling);
            ModelState.Validate();

            _contractSellingService.Save(selling, _branchContext.Branch.Id);
            return Ok(selling);
        }

        [HttpPost]
        public async Task<IActionResult> Export([FromBody] List<Selling> sellings)
        {
            using (var stream = _excelBuilder.Build(sellings))
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
    }
}