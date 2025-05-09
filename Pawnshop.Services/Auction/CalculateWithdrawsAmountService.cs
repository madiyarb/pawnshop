using System;

namespace Pawnshop.Services.Auction
{
    /// <summary>
    /// Сервис расчёта сумм для списания или погашения по Аукциону
    /// </summary>
    public static class CalculateWithdrawsAmountService
    {
        public static CalculatedAuctionSum CalculateSum(ref decimal availableSum, decimal calculatedSum)
        {
            var result = new CalculatedAuctionSum();
            calculatedSum = Math.Abs(calculatedSum); 
            
            if (availableSum <= 0)
            {
                result.ToWithdraw = calculatedSum;
                return result;
            }

            if (availableSum >= calculatedSum)
            {
                result.ToPayOff = calculatedSum;
                availableSum -= calculatedSum;
            }
            else
            {
                result.ToPayOff = availableSum;
                result.ToWithdraw = calculatedSum - availableSum;
                availableSum = 0;
            }

            return result;
        }
    }
}