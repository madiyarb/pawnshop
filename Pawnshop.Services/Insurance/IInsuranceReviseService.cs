using Microsoft.AspNetCore.Http;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;

namespace Pawnshop.Services.Insurance
{
    public interface IInsuranceReviseService : IBaseService<InsuranceRevise>
    {
        InsuranceRevise CreateInsuranceRevise(InsuranceReviseRequest req);
    }
}