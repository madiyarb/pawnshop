using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.Asset;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.AssetView)]
    public class AssetController : Controller
    {
        private readonly AssetRepository _repository;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;

        public AssetController(AssetRepository repository, BranchContext branchContext, ISessionContext sessionContext)
        {
            _repository = repository;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/asset/list")]
        public ListModel<Asset> List([FromBody] ListQueryModel<AssetListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<AssetListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new AssetListQueryModel();
            listQuery.Model.BranchId = _branchContext.Branch.Id;

            return new ListModel<Asset>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost("/api/asset/card")]
        public Asset Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost("/api/asset/save")]
        [Authorize(Permissions.AssetManage)]
        public Asset Save([FromBody] Asset model)
        {
            if (model.Id == 0)
            {
                model.BranchId = _branchContext.Branch.Id;
                model.CreateDate = DateTime.Now;
                model.UserId = _sessionContext.UserId;
            }

            ModelState.Clear();
            TryValidateModel(model);
            ModelState.Validate();

            if (model.Id > 0)
            {
                _repository.Update(model);
            }
            else
            {
                _repository.Insert(model);
            }
            return model;
        }

        [HttpPost("/api/asset/delete")]
        [Authorize(Permissions.AssetManage)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}
