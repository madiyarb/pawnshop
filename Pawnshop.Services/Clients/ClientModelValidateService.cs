using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.LoanSettings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using Pawnshop.Services.CardTopUp.GetTransactionStatusCode;
using System.Text;

namespace Pawnshop.Services.Clients
{
    public class ClientModelValidateService : IClientModelValidateService
    {
        private readonly ClientLegalFormValidationFieldRepository _validationFieldRepository;
        private readonly ClientLegalFormRequiredDocumentRepository _requiredDocumentRepository;
        private readonly AddressTypeRepository _addressTypeRepository;
        private readonly ClientRepository _clientRepository;

        private readonly List<string> FIOFields = new List<string>()
            {Constants.Surname, Constants.Name, Constants.Patronymic, Constants.MaidenName, Constants.FullName};

        public ClientModelValidateService(
            ClientLegalFormValidationFieldRepository validationFieldRepository,
            ClientLegalFormRequiredDocumentRepository requiredDocumentRepository,
            AddressTypeRepository addressTypeRepository,
            ClientRepository clientRepository)
        {
            _validationFieldRepository = validationFieldRepository;
            _requiredDocumentRepository = requiredDocumentRepository;
            _addressTypeRepository = addressTypeRepository;
            _clientRepository = clientRepository;
        }

        public void ValidateClientModel(Client client, bool isOnline = false, bool printCheck = false)
        {
            if (string.IsNullOrWhiteSpace(client.FullName))
                throw new PawnshopApplicationException("Поле Полное имя клиента не заполнено");

            if (client.LegalFormId == 0)
                throw new PawnshopApplicationException("Поле Правовая форма клиента не заполнено");

            if (client.ChiefId.HasValue)
            {
                client.Chief = _clientRepository.Get(client.ChiefId.Value);
                ValidateClientModel(client.Chief);
            }

            var validationErrors = new List<string>
            {
                $"Клиент {client.FullName}: "
            };

            ValidateClientFields(client, validationErrors);

            ValidateClientDocuments(client, validationErrors, isOnline, printCheck);

            ValidateAddresses(client, validationErrors, printCheck);

            ValidateRequisites(client, validationErrors);

            if (validationErrors.Count > 1)
                throw new PawnshopApplicationException(validationErrors.ToArray());
        }

        public void ValidateMerchantClientModel(Client client)
        {
            var validationErrors = new List<string>
            {
                $"Клиент {client.FullName}: "
            };

            if (string.IsNullOrWhiteSpace(client.FullName))
                throw new PawnshopApplicationException("Поле Полное имя клиента не заполнено");

            if (client.LegalFormId == 0)
                throw new PawnshopApplicationException("Поле Правовая форма клиента не заполнено");

            if (client.LegalForm.Code == Constants.INDIVIDUAL)
            {
                //check
                ValidateFIO(Constants.FullName, Constants.FullName, validationErrors, client.FullName);

                if (client.BirthDay == null)
                {
                    validationErrors.Add("Дата рождения не должна быть пустой");
                }
                else
                {
                    ValidateBirthDay(client, validationErrors, (DateTime)client.BirthDay);
                }

                ValidateCountry(client, validationErrors);

                ValidateIdentityNumber(client, validationErrors, client.IdentityNumber);

                ValidateClientDocuments(client, validationErrors);
            }
            else
            {
                ValidateCountry(client, validationErrors);

                if (string.IsNullOrEmpty(client.ChiefName))
                {
                    validationErrors.Add("ФИО первого руководителя не должно быть пустым");
                }

                ValidateIdentityNumber(client, validationErrors, client.IdentityNumber);

                ValidateRequisites(client, validationErrors);

                if (!client.BeneficiaryCode.HasValue)
                {
                    validationErrors.Add("КБе не должен быть пустым");
                }
            }

            if (validationErrors.Count > 1)
            {
                var errorMessagesString = ExtractValidationErrorsToString(validationErrors);
                throw new PawnshopApplicationException(errorMessagesString);
            }
        }

        public void ValidateMobileAppClientModel(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.FullName))
                throw new PawnshopApplicationException("Поле Полное имя клиента не заполнено");

            if (client.LegalFormId == 0)
                throw new PawnshopApplicationException("Поле Правовая форма клиента не заполнено");

            if (client.ChiefId.HasValue)
            {
                client.Chief = _clientRepository.Get(client.ChiefId.Value);
                ValidateClientModel(client.Chief);
            }

            var validationErrors = new List<string>
            {
                $"Клиент {client.FullName}: "
            };

            ValidateClientFields(client, validationErrors);

            ValidateClientDocuments(client, validationErrors);

            if (validationErrors.Count > 1)
                throw new PawnshopApplicationException(validationErrors.ToArray());
        }

        private bool IsClientForValidateFIO(Client client) => client.LegalForm.Code.Equals(Constants.INDIVIDUAL) ||
                   client.LegalForm.Code.Equals(Constants.MICRO_ENTREPRENEUR_NOT_REGISTERED);

        private List<string> ValidateClientFields(Client client, List<string> validationErrors)
        {
            var validationFields = _validationFieldRepository.ListByLegalForm(client.LegalFormId)
                .Select(t => t.FieldCode).ToList();

            foreach (var property in typeof(Client).GetProperties())
            {
                var requiredField = validationFields.Contains(property.Name);

                var propertyValue = property.GetValue(client);

                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                    .Cast<DisplayNameAttribute>().FirstOrDefault();

                bool isEmptyStringOrNull = propertyValue == null;

                if (propertyValue != null)
                {
                    isEmptyStringOrNull = propertyValue.GetType() == typeof(string) &&
                                          string.IsNullOrWhiteSpace(propertyValue.ToString());

                    if (IsClientForValidateFIO(client) && FIOFields.Contains(property.Name) && !isEmptyStringOrNull)
                        ValidateFIO(attribute.DisplayName, property.Name, validationErrors, propertyValue.ToString());

                    if (property.Name == Constants.BirthDay)
                        ValidateBirthDay(client, validationErrors, (DateTime)propertyValue);

                    if (property.Name == Constants.CitizenshipId)
                        ValidateCountry(client, validationErrors);

                    if (property.Name == Constants.CheifId)
                        ValidateCheifId(client, validationErrors);

                    if (property.Name == Constants.IdentityNumber && !isEmptyStringOrNull)
                        ValidateIdentityNumber(client, validationErrors, propertyValue.ToString());
                }

                if (requiredField && isEmptyStringOrNull)
                    validationErrors.Add($"Не заполнено поле {attribute.DisplayName}");
            }

            return validationErrors;
        }

        private void ValidateClientDocuments(Client client, List<string> validationErrors, bool isOnline = false, bool printCheck = false)
        {
            var requiredDocuments = _requiredDocumentRepository.ListByLegalForm(client.LegalFormId, client.IsResident)
                .ToDictionary(t => t.DocumentType.Code);

            if (requiredDocuments.Any())
            {
                ClientDocument findOneRequiredDocument = null;

                if (isOnline || printCheck)
                    findOneRequiredDocument = client.Documents.FirstOrDefault(t => requiredDocuments.Keys.Contains(t.DocumentType.Code));
                else
                    findOneRequiredDocument = client.Documents.FirstOrDefault(t =>
                        requiredDocuments.Keys.Contains(t.DocumentType.Code) && (string.IsNullOrWhiteSpace(t.DocumentType.DateExpirePlaceholder)
                        || (t.DateExpire.HasValue && t.DateExpire.Value.Date > DateTime.Now.Date))
                        );

                if (findOneRequiredDocument == null)
                    validationErrors.Add(
                        $"Не найден обязательный действующий документ ({string.Join(",", requiredDocuments.Values.Select(t => t.DocumentType.Name))})");
            }

            if (client.Documents.Any())
                client.Documents.ForEach(t => ValidateClientDocument(t, validationErrors, isOnline, printCheck));
        }

        private void ValidateClientDocument(ClientDocument clientDocument, List<string> validationErrors, bool isOnline = false, bool printCheck = false)
        {
            if (!string.IsNullOrWhiteSpace(clientDocument.DocumentType.ProviderPlaceholder) && !clientDocument.ProviderId.HasValue)
                validationErrors.Add($"{clientDocument.DocumentType.ProviderPlaceholder} документа {clientDocument.DocumentType.Name} не заполнен");

            if (!string.IsNullOrWhiteSpace(clientDocument.DocumentType.DatePlaceholder) && clientDocument.Date is null || clientDocument.Date == DateTime.MinValue)
                validationErrors.Add($"Дата выдачи документа {clientDocument.DocumentType.Name} не заполнена");

            if (!string.IsNullOrWhiteSpace(clientDocument.DocumentType.DatePlaceholder) && clientDocument.Date > DateTime.Now.Date)
                validationErrors.Add($"Дата выдачи документа {clientDocument.DocumentType.Name} не может быть больше текущей даты");

            if (!string.IsNullOrWhiteSpace(clientDocument.DocumentType.DateExpirePlaceholder) && clientDocument.Date?.AddYears(Constants.DOCUMENT_PERIOD) < clientDocument.DateExpire)
                validationErrors.Add($"Дата окончания срока документа {clientDocument.DocumentType.Name} больше ожидаемого");

            if (!string.IsNullOrWhiteSpace(clientDocument.DocumentType.NumberPlaceholder) && string.IsNullOrEmpty(clientDocument.Number))
                validationErrors.Add($"{clientDocument.DocumentType.NumberPlaceholder} не заполнен для {clientDocument.DocumentType.Name}");

            if (!string.IsNullOrWhiteSpace(clientDocument.DocumentType.NumberPlaceholder) && !string.IsNullOrWhiteSpace(clientDocument.DocumentType.NumberMask))
            {
                string pattern = clientDocument.DocumentType.NumberMask.Replace("\\\\", "\\");
                Regex rg = new Regex(pattern);
                MatchCollection matchedNumber = rg.Matches(clientDocument.Number);

                if (matchedNumber.Count == 0)
                    validationErrors.Add($"{clientDocument.DocumentType.NumberMaskError}");
            }

            if (!isOnline && !printCheck)
            {
                if (!string.IsNullOrWhiteSpace(clientDocument.DocumentType.DateExpirePlaceholder) && clientDocument.DateExpire is null || clientDocument.DateExpire == DateTime.MinValue)
                    validationErrors.Add($"Дата окончания срока документа {clientDocument.DocumentType.Name} не заполнена");

                if (!string.IsNullOrWhiteSpace(clientDocument.DocumentType.BirthPlacePlaceholder) && string.IsNullOrEmpty(clientDocument.BirthPlace))
                    validationErrors.Add($"{clientDocument.DocumentType.BirthPlacePlaceholder} не заполнено для {clientDocument.DocumentType.Name}");
            }
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
                validationErrors.Add($"Неподдерживаемые символы в поле {fieldName}: \"{value}\"");

            if (!regexCyrillic.IsMatch(value) && !regexLatin.IsMatch(value))
                validationErrors.Add($"Поле {fieldName} должно содержать только кириллицу ИЛИ только латиницу");
        }

        public List<string> ValidateFIO(string fieldName, string fieldCode, string value)
        {
            List<string> validationErrors = new List<string>();
            var regex = new Regex(fieldCode == Constants.FullName
                ? Constants.CLIENT_FULLNAME_REGEX
                : Constants.CLIENT_NAME_REGEX);
            var regexCyrillic = new Regex(Constants.CLIENT_NAME_CYRILLIC_REGEX);
            var regexLatin = new Regex(Constants.CLIENT_NAME_LATIN_REGEX);

            if (fieldCode == Constants.Patronymic && value.Length < 3)
                validationErrors.Add($"Поле отчество должно содержать не менее 3 символов");

            if (!regex.IsMatch(value))
                validationErrors.Add($"Неподдерживаемые символы в поле {fieldName}: \"{value}\"");

            if (!regexCyrillic.IsMatch(value) && !regexLatin.IsMatch(value))
                validationErrors.Add($"Поле {fieldName} должно содержать только кириллицу ИЛИ только латиницу");
            return validationErrors;
        }


        private void ValidateCheifId(Client client, List<string> validationErrors)
        {
            if (client.LegalForm != null && !client.LegalForm.IsIndividual &&
                client.Chief != null && client.Chief.LegalForm != null && !client.Chief.LegalForm.IsIndividual)
                validationErrors.Add($"Первый руководитель не может быть юр.лицом");
            else if (client.LegalForm != null && client.LegalForm.IsIndividual)
                validationErrors.Add($"Поле Первый руководитель для физ. лица должно быть пустым");
        }

        private void ValidateBirthDay(Client client, List<string> validationErrors, DateTime value)
        {
            if (client.LegalForm.HasBirthDayValidation)
            {
                var minDate = new DateTime(1915, 1, 1);
                var maxDate = DateTime.Now.AddYears(-18);

                if (value.Date <= minDate.Date)
                    validationErrors.Add($"Поле дата рождения меньше минимальной даты ({minDate:dd.MM.yyyy})");
                else if (value.Date >= maxDate.Date)
                    validationErrors.Add(
                        $"Некорректная дата рождения ({maxDate:dd.MM.yyyy}) или клиент несовершеннолетний ");
            }
            else
            {
                var minDate = new DateTime(1904, 1, 1);
                var maxDate = DateTime.Now;

                if (value.Date <= minDate.Date)
                    validationErrors.Add($"Поле дата регистрации меньше минимальной даты ({minDate:dd.MM.yyyy})");
                else if (value.Date >= maxDate.Date)
                    validationErrors.Add($"Некорректная дата регистрации ({maxDate:dd.MM.yyyy})");
            }
        }

        private void ValidateCountry(Client client, List<string> validationErrors)
        {
            var country = client.Citizenship;

            if (!client.IsResident && country.Code.Contains("KAZ"))
                validationErrors.Add($"Не резидент не может иметь гражданство Казахстана");
        }

        public void ValidateIdentityNumber(Client client, List<string> validationErrors, string value)
        {
            if (!client.LegalForm.HasIINValidation)
            {
                if (value.Length != 12)
                {
                    validationErrors.Add("Длина ИИН/БИН должна быть равна 12 символам!");
                }
                
                //БИН не должен содержать 6 в пятом разряде
                if (value.Substring(4, 1).Contains("6"))
                    validationErrors.Add($"Ошибка валидации БИН клиента, в пятом разряде не должна быть \"6\"");
                //символ НЕ может быть 0,1,2,3
                else if (value.Substring(4, 1).Contains("0") || value.Substring(4, 1).Contains("1") ||
                         value.Substring(4, 1).Contains("2") || value.Substring(4, 1).Contains("3"))
                    validationErrors.Add($"Ошибка валидации БИН клиента, пятый символ не может быть 0,1,2,3");
            }

            //проверка на контрольный разряд
            if (ValidateIdentityNumber(value))
                validationErrors.Add($"ИИН/БИН клиента не прошёл проверку на контрольный разряд");

            if (!client.IdentityNumberIsValid)
                client.IdentityNumberIsValid = true;
        }

        public bool ValidateIdentityNumber(string number)
        {
            //Веса
            try
            {
                var b1 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                var b2 = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 1, 2 };

                var a = new int[12];
                var controll = 0;
                for (var i = 0; i < 12; i++)
                {
                    var num = number.Substring(i, 1);
                    if (!int.TryParse(num, out a[i]))
                    {
                        throw new PawnshopApplicationException(
                            $"Ошибка проверки ИИН клиента, не могу {i} знак конвертировать в число");
                    }

                    if (i < 11) controll += a[i] * b1[i];
                }

                controll %= 11;

                if (controll == 10)
                {
                    controll = 0;
                    for (var i = 0; i < 11; i++)
                        controll += a[i] * b2[i];
                    controll %= 11;
                }

                return controll != a[11];

            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ValidateAddresses(Client client, List<string> validationErrors, bool printCheck = false)
        {
            if (printCheck)
                return;

            var addressTypes = _addressTypeRepository.List(new ListQuery());
            var addressTypesByLegalForm = new Dictionary<string, AddressType>();

            if (client.LegalForm.IsIndividual)
                addressTypesByLegalForm =
                    addressTypes.Where(t => t.IsIndividual && t.IsMandatory).ToDictionary(t => t.Code);
            else
                addressTypesByLegalForm =
                    addressTypes.Where(t => !t.IsIndividual && t.IsMandatory).ToDictionary(t => t.Code);

            if (addressTypesByLegalForm.Any())
            {
                var notAddedAddresses = addressTypesByLegalForm.Where(t => !client.Addresses.Any(a => a.AddressType.Code.Equals(t.Key)));

                if (notAddedAddresses.Count() > 0)
                    validationErrors.Add(
                        $"Не найдены все обязательные актуальные адреса ({string.Join(",", notAddedAddresses.Select(t => t.Value.Name))})");
            }
        }

        private void ValidateRequisites(Client client, List<string> validationErrors)
        {
            if (!client.LegalForm.IsIndividual && (client.Requisites == null || client.Requisites.Count() == 0))
                validationErrors.Add($"Не найдены обязательные реквизиты");
        }

        private string ExtractValidationErrorsToString(List<string> validationErrors)
        {
            var builder = new StringBuilder();

            foreach (var errorMessage in validationErrors)
            {
                builder.Append(errorMessage).Append(", ");
            }
            var errorMessagesString = builder.ToString();
            return errorMessagesString;
        }
    }
}