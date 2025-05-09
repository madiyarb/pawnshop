using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmBitrixIVRModel
    {
        public int ClientId { get; set; }
        public List<CrmBitrixIVROverduePayment> overduePayments { get; set; }
    }
}