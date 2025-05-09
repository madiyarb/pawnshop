using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models.Regions;

namespace Pawnshop.Services.Regions
{
    public class RegionService : IRegionService
    {
        private readonly IRegionRepository _regionRepository;

        public RegionService(IRegionRepository regionRepository)
        {
            _regionRepository = regionRepository;
        }

        public async Task<List<Region>> GetListAsync()
        {
            return await _regionRepository.GetListAsync();
        }
    }
}