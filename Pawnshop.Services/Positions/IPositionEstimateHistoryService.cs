using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Positions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Positions
{
    public interface IPositionEstimateHistoryService
    {
        Task<List<PositionEstimateHistory>> GetPositionEstimateHistory(int positionId);
        Task SavePositionEstimateToHistoryForContract(Contract contract);
        Task ValidateDateAndEstimatedCost(Contract contract);
    }
}
