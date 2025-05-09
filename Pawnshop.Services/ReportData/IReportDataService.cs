using Pawnshop.Data.Models.ReportData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.ReportDatas
{
    public interface IReportDataService
    {
        ReportDataResponseModel Create(ReportDataModel model, int organisationId, int branchId);
    }

}
