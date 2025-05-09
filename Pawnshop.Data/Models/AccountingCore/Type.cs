using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class Type : Pawnshop.AccountingCore.Models.Type, IEntity
    {
        public Type()
        {

        }

        public Type(Pawnshop.AccountingCore.Models.Type model)
        {
            Id = model.Id;
            Name = model.Name;
            NameAlt = model.NameAlt;
            Code = model.Code;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            DeleteDate = model.DeleteDate;
            ParentId = model.ParentId;
            Parent = model.Parent;
            TypeGroup = model.TypeGroup;

        }
    }
}
