using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Cars
{
    public interface IVehcileService
    {
        void BodyNumberValidate(string bodyNumber);
        void TechPassportNumberValidate(string techPassportNumber);
        void TechPassportDateValidate(DateTime? techPassportDate);
        void TransportNumberValidate(string transportNumber);
        void ReleaseYearValidate(int ReleaseYear);
        void MarkValidate(VehicleMark mark);
        void ModelValidate(VehicleModel model);
    }
}
