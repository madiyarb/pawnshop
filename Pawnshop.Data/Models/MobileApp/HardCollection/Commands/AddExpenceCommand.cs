using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class AddExpenceCommand : GetLogNotification, IRequest<bool>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public string ExpenceCode { get; set; }
        public int Cost { get; set; }
        public int AuthorId { get; set; }
        public string Note { get; set; }
        public int BranchId { get; set; }

        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.AddExpence,
                ActionName = HardCollectionActionTypeEnum.AddExpence.ToString(),
                AuthorId = AuthorId,
                Value = value,
                Comment = Note,
                CreateDate = DateTime.Now
            };
        }
    }
}
