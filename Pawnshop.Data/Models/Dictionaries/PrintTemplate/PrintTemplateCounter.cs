using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries.PrintTemplate;

namespace Pawnshop.Data.Models.Dictionaries.PrintTemplates
{
    public class PrintTemplateCounter : PrintTemplateCounterFilter, IEntity 
    {
        public PrintTemplateCounter(PrintTemplateCounterFilter filter, PrintTemplateCounterConfig config)
        {
            ConfigId = config.Id;
            OrganizationId = config.RelatesOnOrganization ? filter.OrganizationId : null;
            BranchId = config.RelatesOnBranch ? filter.BranchId : null;
            CollateralType = config.RelatesOnCollateralType ? filter.CollateralType : null;
            ProductTypeId = config.RelatesOnProductType ? filter.ProductTypeId : null;
            Year = config.RelatesOnYear ? filter.Year : null;
            ScheduleType = config.RelatesOnScheduleType ? filter.ScheduleType : null;
            Counter = config.BeginFrom;
        }
        public PrintTemplateCounter(){}

        public int Id { get; set; }

        /// <summary>
        /// Счетчик
        /// </summary>
        public int Counter { get; set; }
    }
}
