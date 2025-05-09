using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class UpdateCollectionToLegalHardCommand : GetLogNotification, IRequest<bool>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public int ReasonId { get; set; }
        public int AuthorId { get; set; }
        public string Comment { get; set; }

        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.SendLegalCollection,
                ActionName = HardCollectionActionTypeEnum.SendLegalCollection.ToString(),
                AuthorId = AuthorId,
                Value = value,
                Comment = Comment,
                CreateDate = DateTime.Now
            };
        }
    }
}
