using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.LegalCollection.PrintTemplates
{
    public class LegalCasePrintTemplate
    {
        public int PrintTemplateId { get; set; }
        public int CaseStageAfterId { get; set; }
        public string PrintTemplateCode { get; set; }
        public string Name { get; set; }
        public string AgentInformation { get; set; }
        public string AgentInitials { get; set; }
        public int ArbitrationFee { get; set; }
        public string ArbitrationOrganization { get; set; }
        public string CollateralTypeCode { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
