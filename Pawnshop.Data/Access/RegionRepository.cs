using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Regions;

namespace Pawnshop.Data.Access
{
    public class RegionRepository : RepositoryBase, IRegionRepository
    {
        public RegionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<List<Region>> GetListAsync()
        {
            var sqlQuery = @"
                SELECT 
                    r.*, 
                    g.*
                FROM 
                    Regions r
                LEFT JOIN 
                    Groups g ON r.Id = g.RegionId
                WHERE r.DeleteDate IS NULL";

            var regionsDictionary = new Dictionary<int, Region>();

            var results = await UnitOfWork.Session.QueryAsync<Region, Group, Region>(
                sqlQuery,
                (region, group) =>
                {
                    if (!regionsDictionary.TryGetValue(region.Id, out var regionEntry))
                    {
                        regionEntry = region;
                        regionEntry.Groups = new List<Group>();
                        regionsDictionary.Add(regionEntry.Id, regionEntry);
                    }

                    if (group != null)
                        regionEntry.Groups.Add(group);

                    return regionEntry;
                },
                splitOn: "Id"
            );

            return results?.Distinct().ToList();
        }
    }
}