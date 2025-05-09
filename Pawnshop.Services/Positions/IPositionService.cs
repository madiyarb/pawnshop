using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Positions;
using Pawnshop.Services.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Positions
{
    public interface IPositionService
    {
        Task<PositionAdditionalInfo> GetPositionAdditionalInfo(int positionId);
        Task<IList<ContractsWithTotalLoanCost>> GetActiveContractsForPosition(IList<int> positionIds);
        Task<bool> HasUsedPledge(int positionId);
    }
}
