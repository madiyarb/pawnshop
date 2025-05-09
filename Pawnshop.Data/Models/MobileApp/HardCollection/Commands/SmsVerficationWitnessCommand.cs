using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class SmsVerficationWitnessCommand : GetLogNotification, IRequest<bool>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public int AuthorId { get; set; }
        public int ClientId { get; set; }
        public string OTP { get; set; }

        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.VerifySmsWitness,
                ActionName = HardCollectionActionTypeEnum.VerifySmsWitness.ToString(),
                AuthorId = AuthorId,
                Value = value,
                Comment = string.Empty,
                CreateDate = DateTime.Now
            };
        }
    }
}
