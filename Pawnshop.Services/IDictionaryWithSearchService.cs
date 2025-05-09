using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services
{
    public interface IDictionaryWithSearchService<T1, T2> : IDictionaryService<T1>
    {
        ListModel<T1> List(ListQueryModel<T2> listQuery);
    }
}
