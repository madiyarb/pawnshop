using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public interface IApplicationModel : IClientModel
    {
        public string IdentityNumber { get; set; }
        public int AuthorId { get; set; }
    }
}
