using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Regions;

namespace Pawnshop.Data.Access.Interfaces
{
    public interface IRegionRepository
    {
        public Task<List<Region>> GetListAsync();
    }
}