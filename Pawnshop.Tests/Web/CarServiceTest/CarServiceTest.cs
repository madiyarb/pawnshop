using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access;
using Pawnshop.Services.Cars;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Tests.Web.CarServiceTest
{
    [TestClass()]
    public class CarServiceTest
    {
        private ICarService _carService;
        private CarRepository _carRepository;
        private IVehcileService _vehcileService;

        public CarServiceTest()
        {
            string connectionString = "Data Source=37.18.91.106,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";

            var unitOfWork = new UnitOfWork(connectionString);
            _carRepository = new CarRepository(unitOfWork);
            _vehcileService = new VehcileService();
            _carService = new CarService(_carRepository, _vehcileService);
        }

        [TestMethod()]
        public void ValidateCarModel()
        {
            var exCount = 0;
            
            var cars = _carService.ListWithCount(new Core.Queries.ListQuery { });

            /*foreach (var car in cars.List)
            {
                _vehcileService.TechPassportNumberValidate(car.TechPassportNumber);
                _vehcileService.BodyNumberValidate(car.BodyNumber);
                _vehcileService.TechPassportDateValidate(car.TechPassportDate);
            }*/

            //TechPassportNumberValidate
            /*try
            {
                _vehcileService.TechPassportNumberValidate("");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate(" ");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate("    ");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate("_");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate("-");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate("Jc");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate("JС");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate("J585");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate("_J585");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate("JG5854584");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportNumberValidate("JG58545854");
            }
            catch (Exception ex)
            {
                exCount++;
            }*/

            //BodyNumberValidate
            /*try
            {
                _vehcileService.BodyNumberValidate("");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.BodyNumberValidate("ASС");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.BodyNumberValidate("ASС55525-55212121");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.BodyNumberValidate("ASС55525-55212121S");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.BodyNumberValidate("ASJ55525-55212121");
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.BodyNumberValidate("ASQ55525-55212121");
            }
            catch (Exception ex)
            {
                exCount++;
            }*/

            /*try
            {
                _vehcileService.BodyNumberValidate("ASС55525-55212121");
            }
            catch (Exception ex)
            {
                exCount++;
            }*/

            //TechPassportDateValidate
            /*try
            {
                _vehcileService.TechPassportDateValidate(null);
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportDateValidate(new DateTime(2021,09,10));
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportDateValidate(new DateTime(2021, 09, 09));
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportDateValidate(new DateTime(1990, 09, 09));
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportDateValidate(new DateTime(1986, 09, 09));
            }
            catch (Exception ex)
            {
                exCount++;
            }*/

            try
            {
                _vehcileService.TechPassportDateValidate(new DateTime(1986, 09, 09));
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportDateValidate(new DateTime(1991, 01, 01));
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportDateValidate(new DateTime(2021, 09, 10));
            }
            catch (Exception ex)
            {
                exCount++;
            }

            try
            {
                _vehcileService.TechPassportDateValidate(new DateTime(2021, 09, 11));
            }
            catch (Exception ex)
            {
                exCount++;
            }

            Console.WriteLine(exCount);
        }
    }
}
