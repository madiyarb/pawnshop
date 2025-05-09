using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Regions;

namespace Pawnshop.Services.Regions
{
    public interface IRegionService
    {
        public Task<List<Region>> GetListAsync();
    }
}