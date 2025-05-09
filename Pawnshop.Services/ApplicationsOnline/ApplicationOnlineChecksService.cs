using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Services.ApplicationsOnline
{
    public class ApplicationOnlineChecksService : IApplicationOnlineChecksService
    {
        private readonly ApplicationOnlineChecksRepository _applicationOnlineChecksRepository;
        private readonly IList<ApplicationOnlineStatus> _applicationOnlineStatusListForChecks;

        public ApplicationOnlineChecksService(ApplicationOnlineChecksRepository applicationOnlineChecksRepository)
        {
            _applicationOnlineChecksRepository = applicationOnlineChecksRepository;
            _applicationOnlineStatusListForChecks = new List<ApplicationOnlineStatus> { ApplicationOnlineStatus.Consideration, ApplicationOnlineStatus.ModificationFromVerification };
        }

        public void ApproveF2FChecks(Guid applicationOnlineId, int authorId)
        {
            var checks = _applicationOnlineChecksRepository.List(null, new { ApplicationOnlineId = applicationOnlineId });
            var f2fChecks = checks.Where(x => x.TemplateCheck.Code == "F2F" || x.TemplateCheck.Code == "F2F_LIVENESS");

            if (!f2fChecks.Any())
                return;

            foreach (var check in f2fChecks)
            {
                check.UpdateDate = DateTime.UtcNow;
                check.UpdateBy = authorId;
                check.Checked = true;
                _applicationOnlineChecksRepository.Update(check);
            }
        }

        public IList<ApplicationOnlineCheck> GetList(Guid applicationOnlineId, string applicationOnlineStatus)
        {
            ApplicationOnlineStatus status = Enum.Parse<ApplicationOnlineStatus>(applicationOnlineStatus);

            var list = _applicationOnlineChecksRepository.List(null, new { ApplicationOnlineId = applicationOnlineId });

            if (_applicationOnlineStatusListForChecks.Contains(status))
            {
                return list.Where(x => x.TemplateCheck.ToManager).ToList();
            }

            return list;
        }

        public bool IsCheckedChecks(Guid applicationOnlineId, string applicationOnlineStatus)
        {
            ApplicationOnlineStatus status = Enum.Parse<ApplicationOnlineStatus>(applicationOnlineStatus);

            var checks = _applicationOnlineChecksRepository.List(null, new { ApplicationOnlineId = applicationOnlineId });

            if (_applicationOnlineStatusListForChecks.Contains(status))
            {
                return !checks.Where(x => x.TemplateCheck.ToManager).Any(x => !x.Checked);
            }

            return !checks.Any(x => !x.Checked);
        }
    }
}
