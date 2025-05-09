using Pawnshop.Core.Extensions;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.OnlineApplications;
using Pawnshop.Web.Models.AbsOnline;

namespace Pawnshop.Web.Converters
{
    public static class OnlineApplicationCarConverter
    {
        public static OnlineApplicationCar ToCarDomainModel(this CreateApplicationRequest rq, OnlineApplicationCar car)
        {
            var yearIsParse = int.TryParse(rq.CarYear, out int carYear);

            car ??= new OnlineApplicationCar();
            car.Mark = rq.CarMark;
            car.Model = rq.CarModel;
            car.ReleaseYear = yearIsParse ? carYear : car.ReleaseYear;
            car.TransportNumber = rq.CarId;

            return car;
        }

        public static OnlineApplicationCar ToCarDomainModel(this UpdateApplicationRequest rq, OnlineApplicationCar car)
        {
            var yearIsParse = int.TryParse(rq.CarYear, out int carYear);

            car ??= new OnlineApplicationCar();
            car.Mark = rq.CarMarkId.IsNullOrEmpty(car.Mark);
            car.Model = rq.CarModelId.IsNullOrEmpty(car.Model);
            car.ReleaseYear = yearIsParse ? carYear : car.ReleaseYear;
            car.TechPassportDate = rq.TechPassportIssueDate.CompareResultForDb(car.TechPassportDate);
            car.TechPassportNumber = rq.TechPassportNumber.IsNullOrEmpty(car.TechPassportNumber);
            car.TransportNumber = rq.CarNumber.IsNullOrEmpty(car.TransportNumber);

            return car;
        }

        public static OnlineApplicationCar ToCarDomainModel(this ApplicationVerificationResultRequest rq, OnlineApplicationCar car)
        {
            car.BodyNumber = rq.CarVin.IsNullOrEmpty(car.BodyNumber);
            car.Color = rq.CarColor.IsNullOrEmpty(car.Color);
            car.TechPassportDate = rq.TechPassportIssueDate.CompareResultForDb(car.TechPassportDate);
            car.TechPassportNumber = rq.TechPassportNumber.IsNullOrEmpty(car.TechPassportNumber);

            return car;
        }

        public static Car ToCarDomainModel(this OnlineApplicationCar car, Car contractCar, int clientId)
        {
            contractCar ??= new Car();
            contractCar.ClientId = clientId;
            contractCar.CollateralType = AccountingCore.Models.CollateralType.Car;
            contractCar.Mark = car.Mark;
            contractCar.Model = car.Model;
            contractCar.ReleaseYear = car.ReleaseYear.Value;
            contractCar.TransportNumber = car.TransportNumber;
            contractCar.MotorNumber = car.MotorNumber;
            contractCar.BodyNumber = car.BodyNumber;
            contractCar.TechPassportNumber = car.TechPassportNumber;
            contractCar.Color = car.Color;
            contractCar.TechPassportDate = car.TechPassportDate.Value;
            contractCar.ParkingStatusId = 1;
            contractCar.VehicleMarkId = car.VehicleMarkId.Value;
            contractCar.VehicleModelId = car.VehicleModelId.Value;

            return contractCar;
        }
    }
}
