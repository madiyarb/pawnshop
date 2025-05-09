using Pawnshop.Data.Models.Clients;
using System;
using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class AddClientAdditionalContactCommand : GetLogNotification, IRequest<int>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public string ContactOwnerFullName { get; set; }
        public string PhoneNumber { get; set; }
        public int ContactOwnerTypeId { get; set; }
        public int AuthorId { get; set; }
        public string Note { get; set; }


        public static explicit operator ClientAdditionalContact(AddClientAdditionalContactCommand command)
        {
            var parameter = new ClientAdditionalContact()
            {
                ClientId = command.ClientId,
                ContactOwnerFullname = command.ContactOwnerFullName,
                PhoneNumber = command.PhoneNumber,
                ContactOwnerTypeId = command.ContactOwnerTypeId,
                AuthorId = command.AuthorId,
                Note = command.Note
            };

            return parameter;
        }

        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.AddContact,
                ActionName = HardCollectionActionTypeEnum.AddContact.ToString(),
                AuthorId = AuthorId,
                Value = value,
                Comment = Note,
                CreateDate = DateTime.Now
            };
        }
    }
}
