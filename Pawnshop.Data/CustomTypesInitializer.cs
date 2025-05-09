using Dapper;
using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data
{
    public class CustomTypesInitializer
    {
        public static void Init()
        {
            SqlMapper.AddTypeHandler(new JsonObjectHandler<PermissionListDefinition>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<GoldContractSpecific>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractActionData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<Configuration>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<InsuranceData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractRefinanceConfig>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<InsuranceRequestData>());
        }
    }
}