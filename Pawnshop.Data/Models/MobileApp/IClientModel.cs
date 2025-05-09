using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public interface IClientModel
    {
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Patronymic { get; set; }
        public DateTime? BirthDay { get; set; }
        public string DocumentTypeCode { get; set; }
        public string BirthPlace { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public DateTime? DocumentDateExpire { get; set; }
        public string DocumentProviderCode { get; set; }
        public string CountryCode { get; set; }
    }
}
