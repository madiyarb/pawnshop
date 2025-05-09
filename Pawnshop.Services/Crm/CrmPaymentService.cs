using Pawnshop.AccountingCore.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Core;

namespace Pawnshop.Services.Crm
{
    public class CrmPaymentService : ICrmPaymentService
    {
        private readonly IRepository<CrmUploadPayment> _crmPaymentRepository;

        public CrmPaymentService(IRepository<CrmUploadPayment> crmPaymentRepository)
        {
            _crmPaymentRepository = crmPaymentRepository;
        }
        public void Enqueue(IContract contract)
        {
            if (contract.CrmPaymentId.HasValue)
                _crmPaymentRepository.Insert(new CrmUploadPayment
                {
                    ContractId = contract.Id,
                    CreateDate = DateTime.Now
                });
        }
    }
}
