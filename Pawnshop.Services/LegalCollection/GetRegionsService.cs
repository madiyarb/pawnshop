using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.Regions;

namespace Pawnshop.Services.LegalCollection
{
    public class GetRegionsService : IGetRegionsService
    {
        private readonly IRegionService _regionService;

        public GetRegionsService(IRegionService regionService)
        {
            _regionService = regionService;
        }
        
        public async Task<List<RegionDto>> GetListAsync()
        {
            var regions = await _regionService.GetListAsync();

            return regions?.Select(r => new RegionDto
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                NameAlt = r.NameAlt,
                Groups = r.Groups?.Select(g => new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    DisplayName = g.DisplayName,
                    RegionId = g.RegionId
                }).ToList()
            }).ToList();
        }
    }
}