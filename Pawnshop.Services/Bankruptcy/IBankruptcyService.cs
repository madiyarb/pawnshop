using System.Threading.Tasks;

namespace Pawnshop.Services.Bankruptcy
{
    public interface IBankruptcyService
    {
        Task CheckIndividualClient(string iin);
        Task CheckCompany(string bin);
    }
}
