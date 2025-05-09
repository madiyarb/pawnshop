using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ApplicationComparer : IEqualityComparer<Application>
    {
        public bool Equals(Application x, Application y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id && x.ClientId == y.ClientId && x.PositionId == y.PositionId && x.AppId == y.AppId && x.ApplicationDate == y.ApplicationDate
                   && x.LightCost == y.LightCost && x.TurboCost == y.TurboCost && x.MotorCost == y.MotorCost && x.WithoutDriving == y.WithoutDriving 
                   && x.IsAddition == y.IsAddition && x.ParentContractId == y.ParentContractId;
        }

        public int GetHashCode(Application application)
        {
            if (Object.ReferenceEquals(application, null)) return 0;

            int hashApplication = application.Id.GetHashCode() ^ application.ClientId.GetHashCode() ^ application.PositionId.GetHashCode()
                                  ^ application.AppId.GetHashCode() ^ application.ApplicationDate.GetHashCode() ^ application.LightCost.GetHashCode()
                                  ^ application.TurboCost.GetHashCode() ^ application.MotorCost.GetHashCode() ^ application.WithoutDriving.GetHashCode()
                                  ^ application.IsAddition.GetHashCode() ^ application.ParentContractId.GetHashCode();

            return hashApplication;
        }
    }
}
