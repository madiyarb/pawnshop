using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.DTOs;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class UpdateNotLiveInAddressCommand : GetLogNotification, IRequest<bool>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public int AuthorId { get; set; }
        public GeoDto Geo { get; set; }
        public string Comment { get; set; }

        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.NotLiveInAddress,
                ActionName = HardCollectionActionTypeEnum.NotLiveInAddress.ToString(),
                AuthorId = AuthorId,
                Value = value,
                Comment = Comment,
                CreateDate = DateTime.Now
            };
        }

        public static explicit operator HCGeoData(UpdateNotLiveInAddressCommand model)
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
