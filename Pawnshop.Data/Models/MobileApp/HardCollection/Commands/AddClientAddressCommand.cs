using MediatR;
using Newtonsoft.Json;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddClientAddressCommand : GetLogNotification, IRequest<int>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public int AddressTypeId { get; set; }
        public int CountryId { get; set; }
        public int AteId { get; set; }
        public int GeonimId { get; set; }
        public string BuildingNumber { get; set; }
        public string RoomNumber { get; set; }
        public string FullPathRus { get; set; }
        public string FullPathKaz { get; set; }
        public int AuthorId { get; set; }
        public bool IsActual { get; set; } = true;
        public string Note { get; set; }


        public static explicit operator ClientAddress(AddClientAddressCommand command)
        {
            return new ClientAddress()
            {
                ClientId = command.ClientId,
                AddressTypeId = command.AddressTypeId,
                CountryId = command.CountryId,
                ATEId = command.AteId,
                GeonimId = command.GeonimId,
                BuildingNumber = command.BuildingNumber,
                RoomNumber = command.RoomNumber,
                FullPathRus = command.FullPathRus,
                FullPathKaz = command.FullPathKaz,
                AuthorId = command.AuthorId,
                IsActual = command.IsActual,
                Note = command.Note,
                CreateDate = DateTime.Now
            };
        }

        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.AddAddress,
                ActionName = HardCollectionActionTypeEnum.AddAddress.ToString(),
                AuthorId = AuthorId,
                Value = value,
                Comment = Note,
                CreateDate = DateTime.Now
            };
        }
    }
}
