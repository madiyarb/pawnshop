using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Pawnshop.Services.Cars
{
    public class VehcileService : IVehcileService
    {
        public void ReleaseYearValidate(int ReleaseYear)
        {
            var rangeAttribute = new RangeAttribute(Constants.MIN_RELEASE_YEAR, DateTime.Now.Year);
            if (!rangeAttribute.IsValid(ReleaseYear))
                throw new PawnshopApplicationException($"Поле год выпуска должно иметь значение от {Constants.MIN_RELEASE_YEAR} до текущего года");
        }

        public void BodyNumberValidate(string bodyNumber)
        {
            if (string.IsNullOrEmpty(bodyNumber))
            {
                throw new PawnshopApplicationException("VIN-код обязательно для заполнения");
            }

            if (bodyNumber.Length > 17 || bodyNumber.Length < 8)
                throw new PawnshopApplicationException("В VIN-коде должно быть менее 8 символов или не более 17 символов");

            if (!Regex.Match(bodyNumber, Constants.VIN_REGEX).Success)
            {
                throw new PawnshopApplicationException($"VIN-код не может содержать символы: \"{Regex.Replace(bodyNumber, Constants.REGEX_NOT_MATCH, String.Empty)}\"");
            }
        }

        public void TechPassportNumberValidate(string techPassportNumber)
        {
            if (string.IsNullOrEmpty(techPassportNumber))
            {
                throw new PawnshopApplicationException("Номер техпаспорта обязательно для заполнения");
            }

            if (!Regex.Match(techPassportNumber, Constants.TECH_PASSPORT_REGEX).Success)
            {
                throw new PawnshopApplicationException($"Номер техпаспорта не соответствует шаблону [Две заглавные буквы латинского алфавита + 8 цифр]");
            }
        }

        public void TransportNumberValidate(string transportNumber)
        {
            if (string.IsNullOrEmpty(transportNumber))
            {
                throw new PawnshopApplicationException("ГРНЗ обязательно для заполнения");
            }

            if (!Regex.Match(transportNumber, Constants.TRANSPORT_NUMBER_REGEX).Success)
            {
                throw new PawnshopApplicationException($"ГРНЗ не соответствует шаблону");
            }
        }

        public void TechPassportDateValidate(DateTime? techPassportDate)
        {
            if (techPassportDate is null)
                throw new PawnshopApplicationException($"Дата техпаспорта не null");

            if (techPassportDate != null && techPassportDate > DateTime.Now)
                throw new PawnshopApplicationException($"Дата техпаспорта не может быть больше текущей даты");

            if (techPassportDate != null && techPassportDate < Constants.TECH_PASSPORT_MIN_DATE)
                throw new PawnshopApplicationException($"Дата техпаспорта не может быть ранее чем {Constants.TECH_PASSPORT_MIN_DATE}");
        }

        public void MarkValidate(VehicleMark mark)
        {
            if (mark == null || mark.IsDisabled)
                throw new PawnshopApplicationException("На данной машине не заполнена марка");
        }

        public void ModelValidate(VehicleModel model)
        {
            if (model == null || model.IsDisabled)
            {
                throw new PawnshopApplicationException("На данной машине не заполнена модель");
            }
        }
    }
}
