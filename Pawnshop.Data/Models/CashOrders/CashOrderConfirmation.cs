using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.CashOrders
{
    public class CashOrderConfirmation : IEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ConfirmedUserId { get; set; }
        public DateTime ConfirmationDate { get; set; }
        public string Note { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
