using MediatR;
using Microsoft.AspNetCore.Http;
using Pawnshop.Data.Models.MobileApp.HardCollection.DTOs;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Commands
{
    public class UploadFileCommand : GetLogNotification, IRequest<bool>, IGetHistoryNotification
    {
        public int ContractId { get; set; }
        public int AuthorId { get; set; }
        public IFormFile File { get; set; }
        public GeoDto Geo { get; set; }
        public int FileRowId { get; set; }

        public HistoryNotification GetHistoryNotification(int? value = null)
        {
            return new HistoryNotification()
            {
                ContractId = ContractId,
                ActionId = (int)HardCollectionActionTypeEnum.SaveAcceptanceCertificate,
                ActionName = HardCollectionActionTypeEnum.SaveAcceptanceCertificate.ToString(),
                AuthorId = AuthorId,
                Value = value,
                CreateDate = DateTime.Now
            };
        }

        public static explicit operator HCGeoData(UploadFileCommand model)
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
