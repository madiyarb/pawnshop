using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Realties
{
    public interface IRealtyService
    {
        ListModel<Realty> ListWithCount(ListQuery listQuery);
        void Validate(Realty realty);
        Realty Save(Realty realty);
        void Delete(int id);
        Realty Get(int id);
        List<DomainValue> GetRealtyTypes();
        List<DomainValue> GetRealtyPurpose();
        List<DomainValue> GetRealtyWallMaterial();
        List<DomainValue> GetRealtyLightning();
        List<DomainValue> GetRealtyColdWaterSupply();
        List<DomainValue> GetRealtyGasSupply();
        List<DomainValue> GetRealtySanitation();
        List<DomainValue> GetRealtyHotWaterSupply();
        List<DomainValue> GetRealtyHeating();
        List<DomainValue> GetRealtyPhoneConnection();
        NotionalRate GetVpm();
    }
}
