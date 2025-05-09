using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Pawnshop.Services.Auction.Mapping.Interfaces;
using System.IO;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using Pawnshop.Services.Auction.Mapping.HttpServices.Interfaces;
using Pawnshop.Data.Access.Auction.Mapping.Interfaces;
using Pawnshop.Data.Models;
using Pawnshop.Data.Models.Auction.Dtos.Mapping;
using Pawnshop.Data.Models.Auction.Mapping;
using Pawnshop.Services.Auction.Interfaces;

// todo удалить после успешного маппинга
namespace Pawnshop.Services.Auction.Mapping
{
    public class AuctionMappingService : IAuctionMappingService
    {
        private readonly IAuctionMappingRepository _repository;
        private readonly IAuctionMappingHttpService _auctionMappingHttpService;
        private readonly ILogger<AuctionMappingService> _logger;
        private readonly ICarAuctionService _carAuctionService;

        public AuctionMappingService(
            IAuctionMappingRepository repository,
            IAuctionMappingHttpService auctionMappingHttpService,
            ILogger<AuctionMappingService> logger,
            ICarAuctionService carAuctionService)
        {
            _repository = repository;
            _auctionMappingHttpService = auctionMappingHttpService;
            _logger = logger;
            _carAuctionService = carAuctionService;
        }

        public async Task<bool> HandleCarFile(IFormFile file)
        {
            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var excelFile = new ExcelPackage(stream);
                var worksheet = excelFile.Workbook.Worksheets.First();
                var rowCount = worksheet.Dimension.Rows;

                int iterator = 1;
                for (int row = 2; row <= rowCount; row++)
                {
                    var excelCarData = new ExcelCarData();

                    try
                    {
                        var contractIdValue = worksheet.Cells[row, 2].Value;
                        if (contractIdValue != null && int.TryParse(contractIdValue.ToString(), out int contractId))
                        {
                            excelCarData.ContractId = contractId;
                        }
                        else
                        {
                            _logger.LogWarning($"Некорректный или отсутствующий ContractId в строке {row}.");
                            throw new Exception($"Некорректный или отсутствующий ContractId в строке {row}.");
                        }

                        var carIdValue = worksheet.Cells[row, 3].Value;
                        if (carIdValue != null && int.TryParse(carIdValue.ToString(), out int carId))
                        {
                            excelCarData.CarId = carId;
                        }
                        else
                        {
                            _logger.LogWarning($"Некорректный или отсутствующий CarId в строке {row}.");
                            throw new Exception($"Некорректный или отсутствующий CarId в строке {row}.");
                        }

                        var clientIdValue = worksheet.Cells[row, 4].Value;
                        if (clientIdValue != null && int.TryParse(clientIdValue.ToString(), out int clientId))
                        {
                            excelCarData.ClientId = clientId;
                        }
                        else
                        {
                            _logger.LogWarning($"Некорректный или отсутствующий ClientId в строке {row}.");
                            throw new Exception($"Некорректный или отсутствующий ClientId в строке {row}.");
                        }

                        var carStatusValue = worksheet.Cells[row, 5].Value?.ToString();
                        if (carStatusValue == "На реализации")
                        {
                            excelCarData.CarStatusId = 2;
                        }
                        else
                        {
                            excelCarData.CarStatusId = 1;
                        }

                        var auctionClientIdValue = worksheet.Cells[row, 6].Value;
                        if (auctionClientIdValue != null && int.TryParse(auctionClientIdValue.ToString(), out int auctionClientId))
                        {
                            excelCarData.AuctionClientId = auctionClientId;
                        }
                        else
                        {
                            excelCarData.AuctionClientId = null;
                        }

                        var auctionNumberValue = worksheet.Cells[row, 7].Value;
                        excelCarData.AuctionNumber = auctionNumberValue?.ToString();

                        var auctionCostValue = worksheet.Cells[row, 8].Value;
                        if (auctionCostValue != null && decimal.TryParse(auctionCostValue.ToString(), out decimal auctionCost))
                        {
                            excelCarData.AuctionCost = auctionCost;
                        }
                        else
                        {
                            excelCarData.AuctionCost = null;
                        }
                        
                        var auctionDateValue = worksheet.Cells[row, 9].Value;
                        if (auctionDateValue != null && DateTimeOffset.TryParseExact(auctionDateValue.ToString(), "dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out var parsedAuctionDate))
                        {
                            excelCarData.AuctionDate = parsedAuctionDate;
                        }
                        else
                        {
                            excelCarData.AuctionDate = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Ошибка при чтении данных из Excel файла, строки {row}.");
                        throw;
                    }

                    var carDetails = await _repository.GetCarDetailsByContractIdAsync(excelCarData.ContractId);

                    if (carDetails == null)
                    {
                        _logger.LogWarning($"Данные не найдены для для договора с ID: {excelCarData.ContractId} в строке {row}.");
                        throw new Exception($"Данные не найдены для договора с ID: {excelCarData.ContractId}.");
                    }

                    var request = CreateEntityCarMappingApi(excelCarData, carDetails);
                    if (row == rowCount)
                    {
                        request.IsFinal = true;
                    }
                    var response = await _auctionMappingHttpService.SendCarDataAsync(request);
                    
                    if (response is null)
                    {
                        _logger.LogError($"Произошла ошибка при попытке записи авто в модуле Аукцион. Итератор {iterator}");
                        throw new Exception($"Произошла ошибка при попытке записи авто в модуле Аукцион. Итератор {iterator}");
                    }

                    if (response.AuctionId.HasValue && response.OrderRequestId.HasValue)
                    {
                        await _carAuctionService.CreateAsync(new CarAuction
                        {
                            ContractId = request.Car.ExternalCarId,
                            AuctionId = response.AuctionId.Value,
                            Cost = request.Auction.AuctionCost,
                            OrderRequestId = response.OrderRequestId.Value,
                            WithdrawCost = request.Auction.AuctionCost
                        });
                    }

                    iterator++;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при обработке Excel файла.", ex);
                throw;
            }
        }

        public async Task<bool> HandleExpenseFile(IFormFile file)
        {
            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var excelFile = new ExcelPackage(stream);
                var worksheet = excelFile.Workbook.Worksheets.First();
                var rowCount = worksheet.Dimension.Rows;

                var iterator = 1;
                for (int row = 2; row <= rowCount; row++)
                {
                    var excelExpenseData = new ExcelExpenseData();

                    try
                    {
                        var contractIdValue = worksheet.Cells[row, 1].Value;
                        if (contractIdValue != null && int.TryParse(contractIdValue.ToString(), out int contractId))
                        {
                            excelExpenseData.ContractId = contractId;
                        }
                        else
                        {
                            _logger.LogWarning($"Некорректный или отсутствующий ContractId в строке {row}.");
                            throw new Exception($"Некорректный или отсутствующий ContractId в строке {row}.");
                        }

                        var carIdValue = worksheet.Cells[row, 2].Value;
                        if (carIdValue != null && int.TryParse(carIdValue.ToString(), out int carId))
                        {
                            excelExpenseData.CarId = carId;
                        }
                        else
                        {
                            _logger.LogWarning($"Некорректный или отсутствующий CarId в строке {row}.");
                            throw new Exception($"Некорректный или отсутствующий CarId в строке {row}.");
                        }

                        var clientIdValue = worksheet.Cells[row, 3].Value;
                        if (clientIdValue != null && int.TryParse(clientIdValue.ToString(), out int clientId))
                        {
                            excelExpenseData.ClientId = clientId;
                        }
                        else
                        {
                            _logger.LogWarning($"Некорректный или отсутствующий ClientId в строке {row}.");
                            throw new Exception($"Некорректный или отсутствующий ClientId в строке {row}.");
                        }

                        var expenseTypeIdValue = worksheet.Cells[row, 4].Value;
                        if (expenseTypeIdValue != null && int.TryParse(expenseTypeIdValue.ToString(), out int expenseTypeId))
                        {
                            excelExpenseData.ExpenseTypeId = expenseTypeId;
                        }
                        else
                        {
                            _logger.LogWarning($"Некорректный или отсутствующий ExpenseTypeId в строке {row}.");
                            throw new Exception($"Некорректный или отсутствующий ExpenseTypeId в строке {row}.");
                        }

                        var costValue = worksheet.Cells[row, 5].Value;
                        if (costValue != null && decimal.TryParse(costValue.ToString(), out decimal cost))
                        {
                            excelExpenseData.Cost = cost;
                        }
                        else
                        {
                            _logger.LogWarning($"Некорректный или отсутствующий Cost в строке {row}.");
                            throw new Exception($"Некорректный или отсутствующий Cost в строке {row}.");
                        }

                        var noteValue = worksheet.Cells[row, 6].Value;
                        excelExpenseData.Note = noteValue?.ToString();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Ошибка при чтении данных из Excel файла, строки {row}.");
                        throw;
                    }

                    var branchId = await _repository.GetBranchIdForExpenseByContractIdAsync(excelExpenseData.ContractId);

                    if (branchId == 0)
                    {
                        _logger.LogWarning($"Данные не найдены для ID {excelExpenseData.ContractId} в строке {row}.");
                        throw new Exception($"Данные не найдены для ID {excelExpenseData.ContractId}.");
                    }

                    var request = CreateEntityExpenseMappingApi(excelExpenseData, branchId);
                    if (row == rowCount)
                    {
                        request.IsFinal = true;
                    }
                    var response = await _auctionMappingHttpService.SendExpenseDataAsync(request);

                    if (!response)
                    {
                        _logger.LogError($"Произошла ошибка при попытке записи авто в модуле Аукцион. Итератор {iterator}");
                        throw new Exception($"Произошла ошибка при попытке записи авто в модуле Аукцион. Итератор {iterator}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при обработке Excel файла.", ex);
                throw;
            }
        }

        private AuctionMappingCarApiDto CreateEntityCarMappingApi(ExcelCarData excelCarData, AuctionCarDetails carDetails)
        {
            var separatedName = SeparateFullName(carDetails.FullName);

            var contract = new CreateContractDto
            {
                Branch = carDetails.Branch,
                ContractNumber = carDetails.ContractNumber,
                ExternalContractId = carDetails.ContractId
            };

            var client = new CreateClientDto
            {
                ExternalClientId = carDetails.ClientId,
                FirstName = separatedName.FirstName,
                LastName = separatedName.LastName,
                MiddleName = separatedName.MiddleName,
                IIN = carDetails.IIN
            };

            var car = new CreateCarModel
            {
                ExternalCarId = carDetails.CarId,
                Color = carDetails.Color,
                VinCode = carDetails.BodyNumber,
                TransportNumber = carDetails.TransportNumber,
                Brand = carDetails.Mark,
                Model = carDetails.Model,
                ReleaseYear = carDetails.ReleaseYear,
                Client = client,
                Contract = contract,
                CarStatusId = excelCarData.CarStatusId
            };

            CreateAuctionModel auction = null;
            if (excelCarData.AuctionClientId != null && excelCarData.AuctionNumber != null && excelCarData.AuctionCost != null)
            {
                int? carAuctionStatusId;
                if (car.CarStatusId == 2)
                {
                    carAuctionStatusId = 3; // Соответствует статусу "APPROVED"
                }
                else
                {
                    carAuctionStatusId = null;
                }

                auction = new CreateAuctionModel
                {
                    AuctionNumber = excelCarData.AuctionNumber,
                    AuctionContractNumber = carDetails.AuctionContractNumber,
                    AuctionContractDate = carDetails.AuctionContractDate,
                    AuctionDate = excelCarData.AuctionDate.Value,
                    AuctionCost = excelCarData.AuctionCost?? 0,
                    Client = client,
                    CarAuctionStatusId = carAuctionStatusId
                };
            }

            var request = new AuctionMappingCarApiDto
            {
                Iterator = excelCarData.Iterator,
                IsFinal = false,
                AuthorId = "1",
                AuthorName = "Администратор",
                Car = car,
                Auction = auction,
            };
            return request;
        }

        private AuctionMappingExpenseApiDto CreateEntityExpenseMappingApi(ExcelExpenseData expenseData, int branchId)
        {
            var request = new AuctionMappingExpenseApiDto
            {
                Iterator = expenseData.Iterator,
                CarId = expenseData.CarId,
                ExpenseTypeId = expenseData.ExpenseTypeId,
                BranchId = branchId,
                Cost = expenseData.Cost,
                Note = expenseData.Note,
                IsFinal = false
            };
            return request;
        }

        private SeparatedClientName SeparateFullName(string fullname)
        {
            var nameParts = fullname.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var result = new SeparatedClientName();

            if (nameParts.Length == 3)
            {
                result.LastName = nameParts[0];
                result.FirstName = nameParts[1];
                result.MiddleName = nameParts[2];
            }
            else
            {
                result.LastName = nameParts[0];
                result.FirstName = nameParts[1];
                result.MiddleName = null;
            }

            return result;
        }

        private class SeparatedClientName
        {
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; } 
        }

        private class ExcelCarData
        {
            public int? Iterator { get; set; }
            public int ContractId {  get; set; }
            /// <summary>
            /// ExternalCarId
            /// </summary>
            public int CarId {  get; set; }
            public int ClientId {  get; set; }
            public string CarStatusString { get; set; }

            private int _carStatusId;
            public int CarStatusId 
            {
                get => _carStatusId;
                set 
                {
                    _carStatusId = value;
                    if (CarStatusString == "На реализации")
                    {
                        _carStatusId = 2;
                    }
                    else if (CarStatusString == "В ожидании")
                    {
                        _carStatusId = 1;
                    }
                } 
            }

            public int? AuctionClientId { get; set; }
            public string AuctionNumber { get; set; }
            public decimal? AuctionCost {  get; set; }
            public DateTimeOffset? AuctionDate {  get; set; }
        }

        private class ExcelExpenseData
        {
            public int Iterator { get; set; }
            public int ContractId { get; set; }
            public int CarId { get; set; }
            public int ClientId { get; set; }
            public int ExpenseTypeId { get; set; }
            public decimal Cost { get; set; }
            public string Note { get; set; }
        }
    }
}

