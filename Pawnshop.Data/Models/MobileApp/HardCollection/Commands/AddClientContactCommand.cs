using MediatR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class AddClientContactCommand : GetLogNotification, IRequest<int>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public string PhoneNumber { get; set; }
        public int ContactTypeId { get; set; }
        public bool IsActual { get; set; }
        public int SourceId { get; set; }
        public int AuthorId { get; set; }
        public string Note { get; set; }


        public static explicit operator ClientContact(AddClientContactCommand command)
        {
            return new ClientContact()
            {
                Address = command.PhoneNumber,
                ClientId = command.ClientId,
                ContactTypeId = command.ContactTypeId,
                IsDefault = false,
                IsActual = command.IsActual,
                AuthorId = command.AuthorId,
                ContactCategoryId = 206,
                ContactCategoryCode = Constants.DOMAIN_VALUE_CONTACT_ACTUALIZED,
                SourceId = command.SourceId,
                Note = command.Note,
                CreateDate = DateTime.Now,
            };
        }

        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.AddActualContact,
                ActionName = HardCollectionActionTypeEnum.AddActualContact.ToString(),
                AuthorId = AuthorId,
                Value = value,
                Comment = Note,
                CreateDate = DateTime.Now
            };
        }
    }
}
