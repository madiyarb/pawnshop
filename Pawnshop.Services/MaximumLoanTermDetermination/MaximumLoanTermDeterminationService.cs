using System;
using Pawnshop.Data.Access;
using Pawnshop.Services.Cars;
using Pawnshop.Services.MaximumLoanTermDetermination.Exceptions;
using Serilog;

namespace Pawnshop.Services.MaximumLoanTermDetermination
{
    public sealed class MaximumLoanTermDeterminationService : IMaximumLoanTermDeterminationService
    {
        private readonly VehicleLiquidityService _vehicleLiquidityService;
        private readonly ContractPeriodVehicleLiquidityRepository _contractPeriodVehicleLiquidityRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly ILogger _logger;

        public MaximumLoanTermDeterminationService(VehicleLiquidityService vehicleLiquidityService, 
            ContractPeriodVehicleLiquidityRepository contractPeriodVehicleLiquidityRepository,
            LoanPercentRepository loanPercentRepository,
            ILogger logger)
        {
            _vehicleLiquidityService = vehicleLiquidityService;
            _contractPeriodVehicleLiquidityRepository = contractPeriodVehicleLiquidityRepository;
            _loanPercentRepository = loanPercentRepository;
            _logger = logger;
        }
        public int Determinate(int productId, MaximumLoanTermCarDeterminationModel? car = null)
        {
            try
            {
                var loanPercentSettings = _loanPercentRepository.Get(productId);
                if (loanPercentSettings == null)
                {
                    throw new MaximumLoanTermCannotDetermineException(
                        $"Can't determine because loan percent settings : {productId} not found in database");
                }

                int loanPercentSettingsMaxTerm =
                    (loanPercentSettings.ContractPeriodTo * (int?)loanPercentSettings.ContractPeriodToType) /
                    (int)loanPercentSettings.PaymentPeriodType ?? 0;

                if (car != null)
                {
                    var carLiquidity = _vehicleLiquidityService.Get(car.CarMarkId, car.CarModelId, car.ReleaseYear);
                    var contractPeriodVehicleLiquidity =
                        _contractPeriodVehicleLiquidityRepository.GetPeriod(car.ReleaseYear, carLiquidity);

                    if (contractPeriodVehicleLiquidity == null)
                    {
                        return loanPercentSettingsMaxTerm;
                    }

                    if (contractPeriodVehicleLiquidity.MaxMonthsCount > loanPercentSettingsMaxTerm)
                    {
                        return loanPercentSettingsMaxTerm;
                    }

                    return contractPeriodVehicleLiquidity.MaxMonthsCount;
                }

                return loanPercentSettingsMaxTerm;
            }
            catch (InvalidOperationException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new MaximumLoanTermCannotDetermineException(
                    $"Can't determine because for loan percent settings : {productId} param PaymentPeriodType is null or incorrect " +
                    $" With inner exception {exception.Message}");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }
    }
}
