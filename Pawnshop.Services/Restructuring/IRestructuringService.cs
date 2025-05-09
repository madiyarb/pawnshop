using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Restructuring;
using System.Threading.Tasks;

namespace Pawnshop.Services.Restructuring
{
    public interface IRestructuringService
    {
        Task SaveRestructuredPaymentSchedule(RestructuringSaveModel restructuringSaveModel, int branchId);
        Task<RestructuredContractPaymentScheduleVm> BuildRestructuredSchedule(RestructuringModel restructuringModel);
    }
}
