using Pawnshop.Data.Models.PensionAge;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.PensionAges
{
    public interface IPensionAgesService
    {
        public PensionAge Save(PensionAge entity);

        public void Delete(int id);

        public void Update(PensionAge entity);

        public double GetMalePensionAge();

        public double GetFemalePensionAge();

        public List<PensionAge> List();

        public PensionAge Get(int id);

    }
}
