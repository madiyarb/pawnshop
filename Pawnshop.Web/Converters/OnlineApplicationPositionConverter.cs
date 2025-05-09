using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.OnlineApplications;
using Pawnshop.Web.Models.AbsOnline;
using System;

namespace Pawnshop.Web.Converters
{
    public static class OnlineApplicationPositionConverter
    {
        public static OnlineApplicationPosition ToPositionDomainModel(this CreateApplicationRequest rq, OnlineApplicationPosition position)
        {
            if (!string.IsNullOrEmpty(rq.CarMark) && !string.IsNullOrEmpty(rq.CarModel))
            {
                position ??= new OnlineApplicationPosition();
                position.Car = rq.ToCarDomainModel(position.Car);
                position.CollateralType = CollateralType.Car;
            }

            return position;
        }

        public static OnlineApplicationPosition ToPositionDomainModel(this UpdateApplicationRequest rq, OnlineApplicationPosition position)
        {
            if (!string.IsNullOrEmpty(rq.CarMarkId) && !string.IsNullOrEmpty(rq.CarModelId))
            {
                position ??= new OnlineApplicationPosition();
                position.Car = rq.ToCarDomainModel(position.Car);
                position.CollateralType = CollateralType.Car;
            }

            return position;
        }

        public static OnlineApplicationPosition ToPositionDomainModel(this ApplicationVerificationResultRequest rq, OnlineApplicationPosition position)
        {
            position.Car = rq.ToCarDomainModel(position.Car);
            position.EstimatedCost = rq.MarketCost ?? position.EstimatedCost;

            if (rq.LTV > 0 && position.EstimatedCost.HasValue && position.EstimatedCost.Value > 0)
            {
                position.LoanCost = Math.Round((decimal)position.EstimatedCost.Value / 100 * rq.LTV, 2);
            }

            return position;
        }
    }
}
