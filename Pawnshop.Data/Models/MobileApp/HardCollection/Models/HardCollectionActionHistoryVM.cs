using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Models
{
    public class HCActionHistoryVM
    {
        public int Id { get; set; }
        public int HCContractStatusId { get; set; }
        public string ActionName { get; set; }
        public int ActionId { get; set; }
        public string Value { get; set; }
        public int AuthorId { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public string GeoData { get; set; }

        public override string ToString()
        {
            return @$"Id={Id}
HCContractStatusId={HCContractStatusId}
ActionName={ActionName}
ActionId={ActionId}
Value={Value}
AuthorId={AuthorId}
Comment={Comment}
CreateDate={CreateDate};
GeoData={GeoData}";
        }
    }
}
