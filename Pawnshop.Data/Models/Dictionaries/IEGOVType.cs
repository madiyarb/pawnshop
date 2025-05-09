using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries
{
    public interface IEGOVType : IEntity
    {
        new int Id { get; set; }
        string NameRus { get; set; }
        string NameKaz { get; set; }
        string ShortNameRus { get; set; }
        string ShortNameKaz { get; set; }
        string Code { get; set; }
        bool IsActual { get; set; }
    }
}
