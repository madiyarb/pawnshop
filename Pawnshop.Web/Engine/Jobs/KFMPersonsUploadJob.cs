using Hangfire;
using Microsoft.EntityFrameworkCore.Internal;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services.KFM;
using Pawnshop.Web.Engine.Audit;
using System;

namespace Pawnshop.Web.Engine.Jobs
{
    public class KFMPersonsUploadJob
    {
        private readonly JobLog _jobLog;
        private readonly KFMPersonRepository _kfmPersonRepository;
        private readonly IKFMService _kfmService;

        public KFMPersonsUploadJob(JobLog jobLog, KFMPersonRepository kfmPersonRepository, IKFMService kfmService)
        {
            _jobLog = jobLog;
            _kfmPersonRepository = kfmPersonRepository;
            _kfmService = kfmService;
        }

        [Queue("kfm")]
        public void Execute()
        {
            _jobLog.Log("KFMPersonsUploadJob", JobCode.Start, JobStatus.Success);
            try
            {
                var persons = _kfmService.GetListAsync().Result;

                if (!persons.Any())
                    return;

                using (var transaction = _kfmPersonRepository.BeginTransaction())
                {
                    _kfmPersonRepository.TruncateTable();

                    _kfmPersonRepository.Insert(persons).Wait();

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _jobLog.Log("KFMPersonsUploadJob", JobCode.End, JobStatus.Failed, EntityType.None, null, null, ex.Message);

            }
            _jobLog.Log("KFMPersonsUploadJob", JobCode.End, JobStatus.Success, EntityType.None);
        }
    }
}
