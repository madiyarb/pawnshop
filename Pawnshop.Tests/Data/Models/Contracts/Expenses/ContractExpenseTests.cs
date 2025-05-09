using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pawnshop.Data.Models.Membership;
using Newtonsoft.Json;
using Pawnshop.Data.Models.CashOrders;
using System;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Contracts.Expenses.Tests
{
    [TestClass()]
    public class ContractExpenseTests
    {
         //[TestMethod()]
        //public void TakeMoneyFromPrepaymentTest_IfEnougnMoneyOnPrepayment()
        //{
        //    // arrange
        //    var mockContract = GenerateContractMock(CollateralType.Car, 3000);

        //    var mockConfiguration = GenerateConfigurationMock(CollateralType.Car);

        //    ContractExpense expense = new ContractExpense()
        //    {
        //        CreditAccountId = 112,
        //        DebitAccountId = 2,
        //        TotalCost = 3000
        //    };

        //    ContractExpense resultExpense = new ContractExpense()
        //    {
        //        CreditAccountId = 112,
        //        DebitAccountId = 2,
        //        TotalCost = 3000,
        //        TotalLeft = 0,
        //        PrepaymentUsed = 3000,
        //        PrepaymentOrder = new CashOrder()
        //        {
        //            OrderType = OrderType.CashOut,
        //            ClientId = 0,
        //            OrderCost = 3000,
        //            DebitAccountId = 114,
        //            CreditAccountId = 2,
        //            Note = $"Возврат суммы клиенту по договору ",
        //            Reason = $"Возврат суммы клиенту по договору ",
        //            RegDate = DateTime.Now.Date,
        //            ApproveDate = DateTime.Now.Date
        //        }
        //    };

        //    // act
        //    expense.TakeMoneyFromPrepayment(mockContract.Object, mockConfiguration.Object);
        //    expense.PrepaymentOrder.RegDate = DateTime.Now.Date;
        //    expense.PrepaymentOrder.ApproveDate = DateTime.Now.Date;

        //    var actionstr = JsonConvert.SerializeObject(expense);
        //    var resultActionstr = JsonConvert.SerializeObject(resultExpense);

        //    // assert
        //    Assert.AreEqual(actionstr, resultActionstr);
        //}

        //[TestMethod()]
        //public void TakeMoneyFromPrepaymentTest_IfNOTEnougnMoneyOnPrepayment()
        //{
        //    // arrange
        //    var mockContract = GenerateContractMock(CollateralType.Car, 1000);

        //    var mockConfiguration = GenerateConfigurationMock(CollateralType.Car);

        //    ContractExpense expense = new ContractExpense()
        //    {
        //        CreditAccountId = 112,
        //        DebitAccountId = 2,
        //        TotalCost = 3000
        //    };

        //    ContractExpense resultExpense = new ContractExpense()
        //    {
        //        CreditAccountId = 112,
        //        DebitAccountId = 2,
        //        TotalCost = 3000,
        //        TotalLeft = 2000,
        //        PrepaymentUsed = 1000,
        //        PrepaymentOrder = new CashOrder()
        //        {
        //            OrderType = OrderType.CashOut,
        //            ClientId = 0,
        //            OrderCost = 1000,
        //            DebitAccountId = 114,
        //            CreditAccountId = 2,
        //            Note = $"Возврат суммы клиенту по договору ",
        //            Reason = $"Возврат суммы клиенту по договору ",
        //            RegDate = DateTime.Now.Date,
        //            ApproveDate = DateTime.Now.Date
        //        }
        //    };

        //    // act
        //    expense.TakeMoneyFromPrepayment(mockContract.Object, mockConfiguration.Object);
        //    expense.PrepaymentOrder.RegDate = DateTime.Now.Date;
        //    expense.PrepaymentOrder.ApproveDate = DateTime.Now.Date;

        //    var actionstr = JsonConvert.SerializeObject(expense);
        //    var resultActionstr = JsonConvert.SerializeObject(resultExpense);

        //    // assert
        //    Assert.AreEqual(actionstr, resultActionstr);
        //}

        //[TestMethod()]
        //public void TakeMoneyFromPrepaymentTest_IfNoMoneyOnPrepayment()
        //{
        //    // arrange
        //    var mockContract = GenerateContractMock(CollateralType.Car, 0);

        //    var mockConfiguration = GenerateConfigurationMock(CollateralType.Car);

        //    ContractExpense expense = new ContractExpense()
        //    {
        //        CreditAccountId = 112,
        //        DebitAccountId = 2,
        //        TotalCost = 3000
        //    };

        //    ContractExpense resultExpense = new ContractExpense()
        //    {
        //        CreditAccountId = 112,
        //        DebitAccountId = 2,
        //        TotalCost = 3000,
        //        TotalLeft = 3000,
        //        PrepaymentUsed = 0,
        //        PrepaymentOrder = new CashOrder()
        //        {
        //            OrderType = OrderType.CashOut,
        //            ClientId = 0,
        //            OrderCost = 0,
        //            DebitAccountId = 114,
        //            CreditAccountId = 2,
        //            Note = $"Возврат суммы клиенту по договору ",
        //            Reason = $"Возврат суммы клиенту по договору ",
        //            RegDate = DateTime.Now.Date,
        //            ApproveDate = DateTime.Now.Date
        //        }
        //    };

        //    // act
        //    expense.TakeMoneyFromPrepayment(mockContract.Object, mockConfiguration.Object);
        //    expense.PrepaymentOrder.RegDate = DateTime.Now.Date;
        //    expense.PrepaymentOrder.ApproveDate = DateTime.Now.Date;

        //    var actionstr = JsonConvert.SerializeObject(expense);
        //    var resultActionstr = JsonConvert.SerializeObject(resultExpense);

        //    // assert
        //    Assert.AreEqual(actionstr, resultActionstr);
        //}

        //private Mock<Contract> GenerateContractMock(CollateralType collateralType, int prepaymentCost)
        //{
        //    var mock = new Mock<Contract>();
        //    mock.Object.CollateralType = collateralType;
        //    mock.Object.Branch = new Group();
        //    mock.Object.ContractData = new ContractData() { PrepaymentCost = prepaymentCost };
        //    return mock;
        //}

        //private Mock<Configuration> GenerateConfigurationMock(CollateralType collateralType)
        //{
        //    var mock = new Mock<Configuration>();
        //    mock.Object.CashOrderSettings = new CashOrderSettings();
        //    mock.Object.CashOrderSettings.ProfitlessAccountId = 1;
        //    mock.Object.CashOrderSettings.CarCollateralSettings = new CollateralSettings();
        //    mock.Object.CashOrderSettings.CarCollateralSettings.PrepaymentSettings = new AccountSettings() { CreditId = 114, DebitId = 2 };
        //    return mock;
        //}
    }
}