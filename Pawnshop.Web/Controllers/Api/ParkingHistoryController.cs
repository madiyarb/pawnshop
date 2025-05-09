using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Parking;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.Collection;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine;
using System.Linq;
using System;
using Pawnshop.Services.Parkings.History;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ParkingManage)]
    public class ParkingHistoryController : Controller
    {
        private readonly IParkingHistoryService _parkingHistoryService;

        public ParkingHistoryController(
            IParkingHistoryService parkingHistoryService
            )
        {
            _parkingHistoryService = parkingHistoryService;
        }

        [HttpPost, Authorize(Permissions.ParkingManage)]
        [Event(EventCode.ParkingHistorySaved, EventMode = EventMode.Response)]
        public ParkingHistory Save([FromBody] ParkingHistory parkingHistory)
        {
            ModelState.Validate();

            return _parkingHistoryService.Save(parkingHistory);
        }
    }
}
