using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Positions
{
    public class PositionAdditionalInfo
    {
        public PositionEstimate PositionEstimate { get; set; }
        public List<PositionSubject> PositionSubjects { get; set; }
        public Client PositionPledger { get; set; }
        public decimal EstimatedCost { get; set; }
        //используется ли позиция договора на подписанном договоре
        public bool HasUsedPledge { get; set; }
        public List<PositionEstimateHistory> PositionEstimateHistory { get; set; }
    }
}
