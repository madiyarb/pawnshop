using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries.Address
{
    public interface IAddressEGOV : IEntity
    {
        new int Id { get; set; }
        string FullPathRus { get; set; }
        string FullPathKaz { get; set; }
        bool? IsActual { get; set; }
        DateTime? ModifyDate { get; set; }
        string RCACode { get; set; }
    }
}
