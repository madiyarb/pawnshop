using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public class ApplicationOnlineTemplateCheck : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        public User Author { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateBy { get; set; }
        public User UpdateAuthor { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public bool IsActual { get; set; }
        public bool IsManual { get; set; }
        public int? Stage { get; set; }
        public bool ToVerificator { get; set; }
        public bool ToManager { get; set; }
        public bool ToTranche { get; set; }
        public string AttributeName { get; set; }
        public string AttributeCode { get; set; }
    }
}
