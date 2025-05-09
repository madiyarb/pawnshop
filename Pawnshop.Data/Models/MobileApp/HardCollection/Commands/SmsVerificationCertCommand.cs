using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.DTOs;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class SmsVerificationCertCommand : GetLogNotification, IRequest<bool>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public int AuthorId { get; set; }
        public int ClientId { get; set; }
        public string OTP { get; set; }
        public GeoDto Geo { get; set; }
        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.VerifySmsCert,
                ActionName = HardCollectionActionTypeEnum.VerifySmsCert.ToString(),
                AuthorId = AuthorId,
                Value = value,
                Comment = string.Empty,
                CreateDate = DateTime.Now
            };
        }

        public static explicit operator HCGeoData(SmsVerificationCertCommand model)
        {
            return new HCGeoData()
            {
                GpsCoordinates = model.Geo.GpsCoordinates,
                AddressText = model.Geo.AddressText,
                CreateDate = DateTime.Now
            };
        }
    }
}
