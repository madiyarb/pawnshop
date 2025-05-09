using MediatR;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;
using System.Collections.Generic;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddWitnessCommand : GetLogNotification, IRequest<int>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public string FullName { get; set; }
        public string IIN { get; set; }
        public string Address { get; set; }
        public DateTime BirthDay { get; set; }
        public string DocumentNumber { get; set; }
        public string MobilePhone { get; set; }
        public int AuthorId { get; set; }

        public static explicit operator Client(AddWitnessCommand command)
        {
            return new Client()
            {
                CardType = CardType.Standard,
                IdentityNumber = command.IIN,
                FullName = command.FullName,
                Addresses = new List<ClientAddress>()
                {
                    new ClientAddress()
                    {
                        AddressTypeId = 5,
                        CountryId = 118,
                        FullPathRus = command.Address,
                        FullPathKaz = command.Address,
                        CreateDate = DateTime.Now,
                        AuthorId = command.AuthorId,
                        IsActual = true
                    }
                },
                DocumentNumber = command.DocumentNumber,
                CreateDate = DateTime.Now,
                AuthorId = command.AuthorId,
                MobilePhone = command.MobilePhone,
                LegalFormId = 16
            };
        }

        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.SendSmsWitness,
                ActionName = HardCollectionActionTypeEnum.SendSmsWitness.ToString(),
                AuthorId = AuthorId,
                Value = value,
                CreateDate = DateTime.Now
            };
        }
    }
}
