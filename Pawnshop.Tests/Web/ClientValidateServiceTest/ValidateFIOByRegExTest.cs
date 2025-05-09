using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.Text.RegularExpressions;
using Pawnshop.Data.Access;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Tests.Web.ClientValidateServiceTest
{
    [TestClass()]
    public class ValidateFIOByRegExTest
    {
        /*
        private ClientRepository _clientRepository;

        public ValidateFIOByRegExTest()
        {
            string connectionString = "Data Source=37.18.91.106,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test_20210518;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";

            IUnitOfWork unitOfWork = new UnitOfWork(connectionString);
            _clientRepository = new ClientRepository(unitOfWork);
        }

        [TestMethod()]
        public void CheckIsIndividual()
        {
            int clientId = 960;

            var validateFIO = IsClientForValidateFIO(clientId);
            Assert.AreEqual(true, validateFIO);

            clientId = 75118; //MICRO_ENTREPRENEUR_NOT_REGISTERED
            validateFIO = IsClientForValidateFIO(clientId);
            Assert.AreEqual(true, validateFIO);

            clientId = 77132; //SOLE_PROPRIETOR
            validateFIO = IsClientForValidateFIO(clientId);
            Assert.AreEqual(false, validateFIO);

            clientId = 77134; //SOLE_PROPRIETOR
            validateFIO = IsClientForValidateFIO(clientId);
            Assert.AreEqual(false, validateFIO);
        }

        private bool IsClientForValidateFIO(int clientId)
        {
            var client = _clientRepository.Get(clientId);
            if (client == null)
                throw new PawnshopApplicationException($"Клиент {clientId} не найден");

            if (!client.LegalForm.Code.Equals(Constants.INDIVIDUAL) && !client.LegalForm.Code.Equals(Constants.MICRO_ENTREPRENEUR_NOT_REGISTERED))
            {
                return false;
            }
            return true;
        }

            [TestMethod()]
        public void ValidateFIOs()
        {
            var validationErrors = new List<string>();

            //Фамилия
            ValidateFIO("Фамилия", "Surname", validationErrors, "У");
            ValidateFIO("Фамилия", "Surname", validationErrors, "Уа");
            ValidateFIO("Фамилия", "Surname", validationErrors, "Ю");
            ValidateFIO("Фамилия", "Surname", validationErrors, "Х");
            ValidateFIO("Фамилия", "Surname", validationErrors, "АТАМҚҰЛОВ");
            ValidateFIO("Фамилия", "Surname", validationErrors, "Атамқұлов");

            //Имя
            ValidateFIO("Имя", "Name", validationErrors, "Х");
            ValidateFIO("Имя", "Name", validationErrors, "У");

            //Отчество
            //ValidateFIO("Отчество", "Patronymic", validationErrors, "У");
            ValidateFIO("Отчество", "Patronymic", validationErrors, "АТАМҚҰЛОВ");

            Assert.AreEqual(0, validationErrors.Count);
        }

        private void ValidateFIO(string fieldName, string fieldCode, List<string> validationErrors, string value)
        {
            var regex = new Regex(fieldCode == Constants.FullName
                ? Constants.CLIENT_FULLNAME_REGEX
                : Constants.CLIENT_NAME_REGEX);
            var regexCyrillic = new Regex(Constants.CLIENT_NAME_CYRILLIC_REGEX);
            var regexLatin = new Regex(Constants.CLIENT_NAME_LATIN_REGEX);

            if (fieldCode == Constants.Patronymic && value.Length < 3)
                validationErrors.Add($"Поле отчество должно содержать не менее 3 символов");

            if (!regex.IsMatch(value))
                validationErrors.Add($"Неподдерживаемые символы в поле {fieldCode}: \"{value}\"");

            if (!regexCyrillic.IsMatch(value) && !regexLatin.IsMatch(value))
                validationErrors.Add($"Поле {fieldCode} должно содержать только кириллицу ИЛИ только латиницу");
        }
        */
    }
}
