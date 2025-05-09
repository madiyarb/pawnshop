using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.Data.Models.Sellings;
using Pawnshop.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Tests.Web.Engine.Calculation
{
    [TestClass()]
    public class CalculateAmountsTest
    {
        /*[TestMethod()]
        public void RoundAmounts()
        {
            var selling = new Selling();
            selling.SellingCost = 780000.00m;
            selling.PriceCost = 734035.38m;

            decimal depoBalance = 19930m;

            decimal sellingAmount = Math.Abs(selling.GetDiffSellingCostAndPriceCost()); //45964.62

            decimal prepaymentCost = Math.Ceiling(Math.Abs(depoBalance - selling.PriceCost));
            Assert.AreEqual(prepaymentCost, 714106m);

            //decimal CalcSellingCost = sellingAmount + prepaymentCost;
            //Assert.AreEqual(CalcSellingCost, selling.SellingCost);

            decimal SellingCost = 420000m;
            decimal PriceCost = 396748.43m;
            depoBalance = 0m;

            var r1 = Math.Ceiling(Math.Abs(depoBalance - PriceCost));
            var r2 = SellingCost - r1;

            Assert.AreEqual(r2, 23251);
        }*/
    }
}
