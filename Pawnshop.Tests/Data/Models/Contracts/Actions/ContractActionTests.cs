using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Pawnshop.Data.Models.Membership;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts.Discounts;

namespace Pawnshop.Data.Models.Contracts.Actions.Tests
{
    [TestClass()]
    public class ContractActionTests
    {
        /*[TestMethod()]
        public void TakeMoneyFromPrepaymentTest_IfEnougnMoneyOnPrepayment()
        {
            // arrange
            var mockContract = GenerateContractMock(CollateralType.Car, 50000);

            var mockConfiguration = GenerateConfigurationMock(CollateralType.Car);

            ContractAction action = new ContractAction()
            {
                Rows = new ContractActionRow[1]{
                    new ContractActionRow()
                    {
                        CreditAccountId = 5,
                        DebitAccountId = 6,
                        Cost = 10000
                    }
                }
            };

            ContractAction resultAction = new ContractAction()
            {

                Rows = new ContractActionRow[1]{
                    new ContractActionRow()
                    {
                        CreditAccountId = 5,
                        DebitAccountId = 1,
                        Cost = 10000
                    }
                },
                Data = new ContractActionData() { PrepaymentUsed = 10000 }
            };

            // act
            action.TakeMoneyFromPrepayment(mockContract.Object, mockConfiguration.Object);

            var actionstr = JsonConvert.SerializeObject(action);
            var resultActionstr = JsonConvert.SerializeObject(resultAction);

            // assert
            Assert.AreEqual(actionstr, resultActionstr);
        }

        [TestMethod()]
        public void TakeMoneyFromPrepaymentTest_IfNOTEnougnMoneyOnPrepayment()
        {
            // arrange
            var mockContract = GenerateContractMock(CollateralType.Car, 5000);

            var mockConfiguration = GenerateConfigurationMock(CollateralType.Car);

            ContractAction action = new ContractAction()
            {
                Rows = new ContractActionRow[1]{
                    new ContractActionRow()
                    {
                        CreditAccountId = 5,
                        DebitAccountId = 6,
                        Cost = 10000
                    }
                }
            };

            ContractAction resultAction = new ContractAction()
            {

                Rows = new ContractActionRow[2]{
                    new ContractActionRow()
                    {
                        CreditAccountId = 5,
                        DebitAccountId = 1,
                        Cost = 5000
                    },
                    new ContractActionRow()
                    {
                        CreditAccountId = 5,
                        DebitAccountId = 6,
                        Cost = 5000
                    }
                },
                Data = new ContractActionData() { PrepaymentUsed = 5000 }
            };

            // act
            action.TakeMoneyFromPrepayment(mockContract.Object, mockConfiguration.Object);

            var actionstr = JsonConvert.SerializeObject(action);
            var resultActionstr = JsonConvert.SerializeObject(resultAction);

            // assert
            Assert.AreEqual(actionstr, resultActionstr);
        }

        [TestMethod()]
        public void TakeMoneyFromPrepaymentTest_IfNoMoneyOnPrepayment()
        {
            // arrange
            var mockContract = GenerateContractMock(CollateralType.Car, 0);

            var mockConfiguration = GenerateConfigurationMock(CollateralType.Car);

            ContractAction action = new ContractAction()
            {
                Rows = new ContractActionRow[1]{
                    new ContractActionRow()
                    {
                        CreditAccountId = 5,
                        DebitAccountId = 6,
                        Cost = 10000
                    }
                }
            };

            ContractAction resultAction = new ContractAction()
            {

                Rows = new ContractActionRow[1]{
                    new ContractActionRow()
                    {
                        CreditAccountId = 5,
                        DebitAccountId = 6,
                        Cost = 10000
                    }
                },
                Data = new ContractActionData() { PrepaymentUsed = 0 }
            };

            // act
            action.TakeMoneyFromPrepayment(mockContract.Object, mockConfiguration.Object);

            var actionstr = JsonConvert.SerializeObject(action);
            var resultActionstr = JsonConvert.SerializeObject(resultAction);

            // assert
            Assert.AreEqual(actionstr, resultActionstr);
        }

        [TestMethod()]
        public void UseDiscountTest_PercentDiscount()
        {
            // arrange
            var mockContract = GenerateContractMock(CollateralType.Car, 0);
            mockContract.Object.LoanPercent = 10;
            //mockContract.Object.PenaltyPercent = 5;

            ContractAction action = new ContractAction()
            {
                ActionType = ContractActionType.Buyout,
                Discount = new ContractDutyDiscount()
                {
                    Discounts = new List<Discount>()
                    {
                        new Discount()
                        {
                            ContractDiscountId = 1,
                            ContractDiscount = new ContractDiscount()
                            {
                                IsTypical = true,
                                PersonalDiscount = new Dictionaries.PersonalDiscount()
                                {
                                    ActionType = ContractActionType.Buyout
                                }
                            },
                            Rows = new List<DiscountRow>()
                            {
                                new DiscountRow()
                                {
                                    PaymentType = AmountType.Loan,
                                    PercentAdjustment = 1
                                }
                            }
                        }
                    }
                }
            };

            // act
            //action.UseDiscount(mockContract.Object);

            // assert
            Assert.AreEqual(mockContract.Object.LoanPercent, 9);
            //Assert.AreEqual(mockContract.Object.PenaltyPercent, 5);
        }

        [TestMethod()]
        public void UseDiscountTest_PenaltyDiscount()
        {
            // arrange
            var mockContract = GenerateContractMock(CollateralType.Car, 0);
            mockContract.Object.LoanPercent = 10;
            //mockContract.Object.PenaltyPercent = 5;

            ContractAction action = new ContractAction()
            {
                ActionType = ContractActionType.Buyout,
                Discount = new ContractDutyDiscount()
                {
                    Discounts = new List<Discount>()
                    {
                        new Discount()
                        {
                            ContractDiscountId = 1,
                            ContractDiscount = new ContractDiscount()
                            {
                                IsTypical = true,
                                PersonalDiscount = new Dictionaries.PersonalDiscount()
                                {
                                    ActionType = ContractActionType.Buyout
                                }
                            },
                            Rows = new List<DiscountRow>()
                            {
                                new DiscountRow()
                                {
                                    PaymentType = AmountType.Penalty,
                                    PercentAdjustment = 5
                                }
                            }
                        }
                    }
                }
            };

            // act
            //action.UseDiscount(mockContract.Object);

            // assert
            Assert.AreEqual(10, mockContract.Object.LoanPercent);
            //Assert.AreEqual(0, mockContract.Object.PenaltyPercent);
        }

        private Mock<Contract> GenerateContractMock(CollateralType collateralType, int prepaymentCost)
        {
            var mock = new Mock<Contract>();
            mock.Object.CollateralType = collateralType;
            mock.Object.Branch = new Group();
            mock.Object.ContractData = new ContractData() { PrepaymentCost = prepaymentCost };
            return mock;
        }

        private Mock<Configuration> GenerateConfigurationMock(CollateralType collateralType)
        {
            var mock = new Mock<Configuration>();
            mock.Object.CashOrderSettings = new CashOrderSettings();
            mock.Object.CashOrderSettings.ProfitlessAccountId = 1;
            mock.Object.CashOrderSettings.CarCollateralSettings = new CollateralSettings();
            mock.Object.CashOrderSettings.CarCollateralSettings.PrepaymentSettings = new AccountSettings() { CreditId = 1, DebitId = 2 };
            return mock;
        }*/

        
    }
}