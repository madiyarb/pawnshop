using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Data.Models.TasLabRecruit;

namespace Pawnshop.Services.TasLabRecruit
{
    public interface ITasLabRecruitService
    {
        Task<List<Recruit>> GetRecruitsList();
        Task<List<Recruit>> GetRecruitsDelta();
        Task<RecruitIINResponse> GetRecruitByIIN(string iin);
        Task<List<Recruit>> GetRecruitsListMKB();
    }
}
