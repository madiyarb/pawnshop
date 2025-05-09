using Pawnshop.Data.Models.Contracts.LegalCollectionCalculations;
using System;
using Pawnshop.Data.Models.Contracts;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.LegalCollectionCalculations
{
    public interface ICalculationLegalCollectionAmountsService
    {
        Task<LegalAmountsViewModel> CalculateLegalAmounts(Contract contract);
    }
}
