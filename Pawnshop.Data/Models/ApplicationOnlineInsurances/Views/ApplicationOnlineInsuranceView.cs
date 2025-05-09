using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.ApplicationOnlineInsurances.Views
{
    public sealed class ApplicationOnlineInsuranceView : ApplicationOnlineInsurance
    {

        public void FillStatus()
        {
            ApplicationOnlineInsuranceStatus insuranceStatus;
            if (Enum.TryParse(Status, out insuranceStatus))
            {
                Status = insuranceStatus.GetDisplayName();
            }
        }
    }
}
