using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Dictionaries.Address;

namespace Pawnshop.Data.Models.Egov
{
    public static class EgovDictionaries
    {
        public static readonly List<EgovDictionary> All = new List<EgovDictionary>();


        static EgovDictionaries()
        {
            Register(typeof(AddressATEType), "d_ats_types", true);
            Register(typeof(AddressGeonimType), "d_geonims_types", true);
            Register(typeof(AddressBuildingType), "d_buildings_pointers", true);
            Register(typeof(AddressRoomType), "d_rooms_types", true);
            Register(typeof(AddressATE), "s_ats");
            Register(typeof(AddressGeonim), "s_geonims");
            Register(typeof(AddressBuilding), "s_buildings");
            Register(typeof(AddressRoom), "s_pb");
        }

        private static void Register(Type type, string outerName, bool isType = false)
        {

            var all = All;
            all.Add(new EgovDictionary
            {
                PawnshopType = type,
                OuterName = outerName,
                IsType = isType
            });
        }
    }
}
