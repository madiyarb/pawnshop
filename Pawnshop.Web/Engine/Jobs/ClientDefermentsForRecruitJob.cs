using Pawnshop.Services.TasLabRecruit;
using Pawnshop.Services.ClientDeferments.Interfaces;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Models.TasLabRecruit;
using System.Collections.Generic;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using Newtonsoft.Json;

namespace Pawnshop.Web.Engine.Jobs
{
    public class ClientDefermentsForRecruitJob
    {
        private readonly IClientDefermentService _clientDefermentService;
        private readonly ITasLabRecruitService _recruitService;
        private readonly JobLog _jobLog;
        private delegate Task<List<Recruit>> GetRecruitsDelegate();

        public ClientDefermentsForRecruitJob(
            IClientDefermentService clientDefermentService,
            ITasLabRecruitService recruitService,
            JobLog jobLog)
        {
            _clientDefermentService = clientDefermentService;
            _recruitService = recruitService;
            _jobLog = jobLog;
        }

        public async Task ExecuteList()
        {
            await Execute(_recruitService.GetRecruitsList, "ClientDefermentsForRecruitListJob");
        }

        public async Task ExecuteDelta()
        {
            await Execute(_recruitService.GetRecruitsDelta, "ClientDefermentsForRecruitDeltaJob");
        }

        public async Task ExecuteListMKB()
        {
            await Execute(_recruitService.GetRecruitsListMKB, "ClientDefermentsForRecruitListMKBJob");
        }

        private async Task Execute(GetRecruitsDelegate GetRecruits, string jobName)
        {
            try
            {
                _jobLog.Log(jobName, JobCode.Start, JobStatus.Success, EntityType.ClientDefermentsForRecruit);
                var recruits = GetRecruits().Result;
                foreach (var recruit in recruits)
                {
                    await _clientDefermentService.RegisterRecruitDeferment(recruit);
                }
                _jobLog.Log(jobName, JobCode.End, JobStatus.Success, EntityType.ClientDefermentsForRecruit);
            }
            catch (Exception ex)
            {
                _jobLog.Log(jobName, JobCode.End, JobStatus.Failed, EntityType.ClientDefermentsForRecruit, responseData: JsonConvert.SerializeObject(ex));
            }
        }
    }
}
