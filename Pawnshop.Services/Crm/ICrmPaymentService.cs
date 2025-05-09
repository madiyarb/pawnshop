using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.Services.Crm
{
    public interface ICrmPaymentService
    {
        void Enqueue(IContract contract);
    }
}
