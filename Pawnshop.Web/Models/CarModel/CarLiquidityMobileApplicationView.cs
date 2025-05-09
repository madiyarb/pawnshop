using Pawnshop.Data.Models.ApplicationsOnlineCar;

namespace Pawnshop.Web.Models.CarModel
{
    public sealed class CarLiquidityMobileApplicationView
    {
        public string Liquidity { get; set; }
        public int MaxMonth { get; set; }

        public CarLiquidityMobileApplicationView()
        {
            Liquidity = "Low";
            MaxMonth = 12;
        }

        public CarLiquidityMobileApplicationView(CarLiquidity liquidity, int maxMonth)
        {
            Liquidity = liquidity.ToString();
            MaxMonth = maxMonth;
        }
    }
}
