using System;
using System.Collections.Generic;
using System.Text;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ClientDebtorRegistryData
{
    [Table("ClientDebtorRegistryData")]
    public sealed class ClientDebtorRegistryData
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        public DateTime CreateDate { get; set; }

        public string CategoryRu { get; set; }

        public string RecoveryAmount { get; set; }

        public string KbkNameRu { get; set; }

        public string DisaDepartmentNameRu { get; set; }

        public string RecovererTypeRu { get; set; }

        public Guid ClientDebtorRegistryRequestId { get; set; }

        public ClientDebtorRegistryData()
        {
            
        }

        public ClientDebtorRegistryData(string categoryRu, string recoveryAmount, string kbkNameRu, string disaDepartmentNameRu, string recovererTypeRu, Guid clientDebtorRegistryRequestId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            CategoryRu = categoryRu;
            RecoveryAmount = recoveryAmount;
            KbkNameRu = kbkNameRu;
            DisaDepartmentNameRu = disaDepartmentNameRu;
            RecovererTypeRu = recovererTypeRu;
            ClientDebtorRegistryRequestId = clientDebtorRegistryRequestId;
        }
    }
}
