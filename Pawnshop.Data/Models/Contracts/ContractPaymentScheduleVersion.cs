using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractPaymentScheduleVersion
    {
        public int Number { get; set; }
        public int ContractId { get; set; }
        public int ActionId { get; set; }
        public DateTime ScheduleDate { get; set; }
		public int? HistoryId { get; set; }
	}
}
