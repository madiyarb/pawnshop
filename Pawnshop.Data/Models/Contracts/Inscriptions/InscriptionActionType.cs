using System;
using System.Collections.Generic;
using System.Text;
namespace Pawnshop.Data.Models.Contracts.Inscriptions
{
    public enum InscriptionActionType : short
    {
        Сreation = 0,
        Confirm = 10,
        Withdraw = 20,
        Execution = 30
    }
}