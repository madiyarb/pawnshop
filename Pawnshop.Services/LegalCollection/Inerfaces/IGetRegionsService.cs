using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface IGetRegionsService
    {
        public Task<List<RegionDto>> GetListAsync();
    }
}