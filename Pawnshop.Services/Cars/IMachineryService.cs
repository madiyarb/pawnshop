using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Cars
{
    public interface IMachineryService
    {
        ListModel<Machinery> ListWithCount(ListQuery listQuery);
        Machinery Get(int id);
        Machinery Save(Machinery machinery);
        void Delete(int id);
        List<string> Colors();
        void Validate(Machinery machinery);
    }
}
