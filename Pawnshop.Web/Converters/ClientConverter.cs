using Pawnshop.Core.Extensions;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Models.AbsOnline;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace Pawnshop.Web.Converters
{
    public static class ClientConverter
    {
        private static readonly List<string> _allowedDocTypes = new List<string> { Constants.IDENTITYCARD, Constants.PASSPORTKZ };

        public static Client ToDomainModel(this CreateApplicationRequest rq, Client client)
        {
            client ??= new Client();

            client.IdentityNumber = rq.IIN;
            client.Surname = rq.Lastname.IsNullOrEmpty(client.Surname);
            client.Name = rq.Firstname.IsNullOrEmpty(client.Name);
            client.Patronymic = rq.Middlename.IsNullOrEmpty(client.Patronymic);
            client.FullName = $"{client.Surname} {client.Name} {client.Patronymic}";
            client.Email = rq.Email.IsNullOrEmpty(client.Email);
            client.MobilePhone = rq.MobilePhone.IsNullOrEmpty(client.MobilePhone);
            client.BirthDay = rq.BirthDate.CompareResultForDb(client.BirthDay);
            client.IsResident = true;
            client.BeneficiaryCode = 19;
            client.CitizenshipId = 118;
            client.LegalFormId = 16;

            FillAddress(client, rq.Address);

            if (client.Id == 0)
            {
                client.AuthorId = 1;
                client.CardType = CardType.Standard;
                client.CreateDate = DateTime.Now;
            }

            return client;
        }

        public static Client ToDomainModel(this UpdateApplicationRequest rq, Client client)
        {
            client.Surname = rq.Lastname.IsNullOrEmpty(client.Surname);
            client.Name = rq.Firstname.IsNullOrEmpty(client.Name);
            client.Patronymic = rq.Middlename.IsNullOrEmpty(client.Patronymic);
            client.FullName = $"{client.Surname} {client.Name} {client.Patronymic}";
            client.BirthDay = rq.BirthDate.CompareResultForDb(client.BirthDay);

            FillDocument(client, rq.PassportNumber, rq.PassportIssueDate);

            return client;
        }

        public static Client ToDomainModel(this ApplicationVerificationResultRequest rq, Client client,
            List<ClientDocumentType> docTypes, int? docProviderId, int? ATEId)
        {
            client.Email = rq.Email.IsNullOrEmpty(client.Email);
            client.IsMale = IsMale(rq.Gender);
            client.MobilePhone = rq.MobilePhone.IsNullOrEmpty(client.MobilePhone);
            client.StaticPhone = rq.AdditionalPhone.IsNullOrEmpty(client.StaticPhone);

            FillAddress(client, rq.ContractAddress, ATEId);
            FillDocument(client, rq.PassportNumber, rq.PassportIssueDate, docTypes, docProviderId);

            return client;
        }


        private static void FillDocument(Client client, string passportNumber, DateTime? issueDate,
            List<ClientDocumentType> docTypes = null, int? docProviderId = null)
        {
            if (string.IsNullOrEmpty(passportNumber))
                return;

            var document = client.Documents.FirstOrDefault(x => x.Number == passportNumber)
                ?? new ClientDocument
                {
                    AuthorId = 1,
                    ClientId = client.Id,
                    CreateDate = DateTime.Now,
                    Number = passportNumber,
                };

            document.Date = issueDate.CompareResultForDb(document.Date);
            document.ProviderId = docProviderId ?? document.ProviderId;

            if (!docTypes?.Any() ?? true)
            {
                if (document.TypeId == 0)
                {
                    document.DocumentType = new ClientDocumentType { Code = Constants.ANOTHER };
                    document.TypeId = 1;
                }
            }
            else
            {
                var docType = docTypes.Where(x => _allowedDocTypes.Contains(x.Code))
                    .FirstOrDefault(x => Regex.IsMatch(passportNumber, x.NumberMask.Replace("\\\\", "\\")))
                    ?? docTypes.FirstOrDefault(x => x.Code == Constants.ANOTHER);

                document.DocumentType = new ClientDocumentType { Code = docType.Code };
                document.TypeId = docType.Id;
            }

            client.Documents = new List<ClientDocument> { document };
        }

        private static void FillAddress(Client client, string address, int? ATEId = null)
        {
            if (string.IsNullOrEmpty(address))
                return;

            client.Addresses.Where(x => x.IsActual && x.FullPathRus != address && (x.AddressTypeId == 5 || x.AddressTypeId == 6))
                .ToList().ForEach(x => x.IsActual = false);

            var baseAddress = client.Addresses.FirstOrDefault(x => x.IsActual && x.AddressTypeId == 5 && x.FullPathRus == address);
            baseAddress = CreateAddressEntity(baseAddress, address, client.Id, 5, ATEId);

            var regAddress = client.Addresses.FirstOrDefault(x => x.IsActual && x.AddressTypeId == 6 && x.FullPathRus == address);
            regAddress = CreateAddressEntity(regAddress, address, client.Id, 6, ATEId);

            if (baseAddress.Id == 0)
                client.Addresses.Add(baseAddress);

            if (regAddress.Id == 0)
                client.Addresses.Add(regAddress);
        }

        private static ClientAddress CreateAddressEntity(ClientAddress address, string addressPath,
            int clientId, int addressTypeId, int? ateId = null)
        {
            address ??= new ClientAddress();
            address.AddressTypeId = addressTypeId;
            address.ATEId = ateId;
            address.AuthorId = 1;
            address.ClientId = clientId;
            address.CountryId = 118;
            address.CreateDate = DateTime.Now;
            address.FullPathKaz = addressPath.IsNullOrEmpty(address.FullPathKaz);
            address.FullPathRus = addressPath.IsNullOrEmpty(address.FullPathRus);
            address.IsActual = true;

            return address;
        }

        private static bool? IsMale(string gender) =>
            gender.ToLower() switch
            {
                "мужчина" => true,
                "женщина" => false,
                _ => null
            };
    }
}
