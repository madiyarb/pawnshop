using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Web.Engine.Calculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.Web.Engine.Calculation.Tests
{
    [TestClass()]
    public class ContractScheduleBuilderTests
    {
        /*[TestMethod()]
        public void BuildTest_CreateSchedule_For_3_Month()
        {
            // arrange
            DateTime beginDate = new DateTime(2020, 1, 1);
            int loanCost = 100000;
            decimal loanPercentPerDay = 0.15M;
            DateTime maturityDate = new DateTime(2020, 4, 1);

            // act
            List<ContractPaymentSchedule> schedule = new ContractScheduleBuilder().BuildAnnuity(beginDate, loanCost, loanPercentPerDay, maturityDate).ToList();
            List<ContractPaymentSchedule> expectedSchedule = new List<ContractPaymentSchedule>();
            expectedSchedule.Add(new ContractPaymentSchedule()
            {
                CreateDate = DateTime.Now,
                Date = new DateTime(2020, 2, 1),
                DebtCost = 31877.336M,
                DebtLeft = 68122.66M,
                PercentCost = 4500,
                Period = 30
            });

            expectedSchedule.Add(new ContractPaymentSchedule()
            {
                CreateDate = DateTime.Now,
                Date = new DateTime(2020, 3, 1),
                DebtCost = 33311.82M,
                DebtLeft = 34810.85M,
                PercentCost = 3065.520M,
                Period = 30
            });

            expectedSchedule.Add(new ContractPaymentSchedule()
            {
                CreateDate = DateTime.Now,
                Date = new DateTime(2020, 4, 1),
                DebtCost = 34810.85M,
                DebtLeft = 0,
                PercentCost = 1566.49M,
                Period = 30
            });

            // assert

            Assert.AreEqual(expectedSchedule.Count, schedule.Count);

            foreach (var item in schedule)
            {
                var expected = expectedSchedule.Where(x => x.Date.Date == item.Date.Date).FirstOrDefault();

                Assert.AreEqual(expected.Date.Date, item.Date.Date);
                Assert.AreEqual(Math.Round(expected.DebtCost, 0), Math.Round(item.DebtCost, 0));
                Assert.AreEqual(Math.Round(expected.DebtLeft, 0), Math.Round(item.DebtLeft, 0));
                Assert.AreEqual(Math.Round(expected.PercentCost, 0), Math.Round(item.PercentCost, 0));
                Assert.AreEqual(expected.Period, item.Period);

            }
        }*/
    }
}