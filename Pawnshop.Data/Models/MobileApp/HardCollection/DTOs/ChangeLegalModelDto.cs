using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.DTOs
{
    public class ChangeLegalModelDto
    {
        public int ContractId { get; set; }
        public int ReasonId { get; set; }
        public int AuthorId { get; set; }
        public string Comment { get; set; }

        public static explicit operator HCActionHistory(ChangeLegalModelDto model)
        {
            return new HCActionHistory()
            {
                ActionId = (int)HardCollectionActionTypeEnum.SendLegalCollection,
                ActionName = HardCollectionActionTypeEnum.SendLegalCollection.ToString(),
                AuthorId = model.AuthorId,
                Comment = model.Comment,
                CreateDate = DateTime.Now
            };
        }
    }
}
