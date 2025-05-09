using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json.Linq;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Data.Models.LegalCollection.PrintTemplates;
using Account = Pawnshop.Data.Models.AccountingCore.Account;

namespace Pawnshop.Data.Access.LegalCollection
{
    public class LegalCollectionRepository : RepositoryBase, ILegalCollectionRepository
    {
        public LegalCollectionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<LegalCaseContractDto> GetLegalCaseContractInfoAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
            SELECT CL.Id              as 'ClientId',
                   CL.IdentityNumber  as 'ClientIdentityNumber',
                   CL.FullName        as 'ClientFullName',
                   CL.Name            as 'ClientName',
                   CL.Surname         as 'ClientSurname',
                   CL.Patronymic      as 'ClientPatronymic',
                   CR.Id              as 'CarId',
                   CR.TransportNumber as 'CarNumber',
                   CR.Mark            as 'CarMark',
                   CR.Model           as 'CarModel',
                   PS.Id              as 'PositionId',
                   PS.StatusName      as 'PositionStatusName',
                   LPS.Id             as 'ProductId',
                   LPS.Name           as 'ProductName',
                   G.Id               as 'BranchId'
            FROM Contracts CNTR
                    JOIN Clients CL on CL.Id = CNTR.ClientId
                    JOIN ContractPositions CP on CNTR.Id = CP.ContractId
                    JOIN Cars CR on Cp.PositionId = CR.Id
                    JOIN ParkingStatuses PS on CR.ParkingStatusId = PS.Id
                    JOIN LoanPercentSettings LPS on CNTR.SettingId = LPS.Id
                    JOIN Groups G on CNTR.BranchId = G.Id
            WHERE ContractId = @ContractId
              AND CNTR.DeleteDate IS NULL
              AND CL.DeleteDate IS NULL
              AND CP.DeleteDate IS NULL
              AND PS.DeleteDate IS NULL
              AND LPS.DeleteDate IS NULL";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync(sqlQuery, parameters, UnitOfWork.Transaction);

            return new LegalCaseContractDto
            {
                BranchId = result != null ? (int)result.BranchId : 0,
                Product = new ProductDto
                {
                    Id = result != null ? (int)result.ProductId : 0,
                    Name = result != null ? result.ProductName : null
                },
                Client = new ClientDto
                {
                    Id = result != null ? (int)result.ClientId : 0,
                    IdentityNumber = result != null ? result.ClientIdentityNumber : null,
                    Name = result != null ? result.ClientName : null,
                    Surname = result != null ? result.ClientSurname : null,
                    Patronymic = result != null ? result.ClientPatronymic : null,
                    FullName = result != null ? result.ClientFullName : null
                },
                Car = new CarDto
                {
                    Id = result != null ? (int)result.CarId : 0,
                    TransportNumber = result != null ? result.CarNumber : null,
                    Mark = result != null ? result.CarMark : null,
                    Model = result != null ? result.CarModel : null,
                    StatusId = result != null ? (int?)result.PositionId : null,
                    StatusName = result != null ? result.PositionStatusName : null
                }
            };
        }

        public LegalCaseContractDto GetLegalCaseContractInfo(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
            SELECT CL.Id              as 'ClientId',
                   CL.IdentityNumber  as 'ClientIdentityNumber',
                   CL.FullName        as 'ClientFullName',
                   CL.Name            as 'ClientName',
                   CL.Surname         as 'ClientSurname',
                   CL.Patronymic      as 'ClientPatronymic',
                   CR.Id              as 'CarId',
                   CR.TransportNumber as 'CarNumber',
                   CR.Mark            as 'CarMark',
                   CR.Model           as 'CarModel',
                   PS.Id              as 'PositionId',
                   PS.StatusName      as 'PositionStatusName',
                   LPS.Id             as 'ProductId',
                   LPS.Name           as 'ProductName',
                   G.Id               as 'BranchId'
            FROM Contracts CNTR
                    JOIN Clients CL on CL.Id = CNTR.ClientId
                    JOIN ContractPositions CP on CNTR.Id = CP.ContractId
                    JOIN Cars CR on Cp.PositionId = CR.Id
                    JOIN ParkingStatuses PS on CR.ParkingStatusId = PS.Id
                    JOIN LoanPercentSettings LPS on CNTR.SettingId = LPS.Id
                    JOIN Groups G on CNTR.BranchId = G.Id
            WHERE ContractId = @ContractId
                AND CNTR.DeleteDate IS NULL
                AND CL.DeleteDate IS NULL
                AND CP.DeleteDate IS NULL
                AND PS.DeleteDate IS NULL
                AND LPS.DeleteDate IS NULL";

            var result = UnitOfWork.Session
                .QueryFirstOrDefault(sqlQuery, parameters, UnitOfWork.Transaction);

            return new LegalCaseContractDto
            {
                BranchId = result != null ? (int)result.BranchId : 0,
                Product = new ProductDto
                {
                    Id = result != null ? (int)result.ProductId : 0,
                    Name = result != null ? result.ProductName : null
                },
                Client = new ClientDto
                {
                    Id = result != null ? (int)result.ClientId : 0,
                    IdentityNumber = result != null ? result.ClientIdentityNumber : null,
                    Name = result != null ? result.ClientName : null,
                    Surname = result != null ? result.ClientSurname : null,
                    Patronymic = result != null ? result.ClientPatronymic : null,
                    FullName = result != null ? result.ClientFullName : null
                },
                Car = new CarDto
                {
                    Id = result != null ? (int)result.CarId : 0,
                    TransportNumber = result != null ? result.CarNumber : null,
                    Mark = result != null ? result.CarMark : null,
                    Model = result != null ? result.CarModel : null,
                    StatusId = result != null ? (int?)result.PositionId : null,
                    StatusName = result != null ? result.PositionStatusName : null
                }
            };
        }

        public async Task<ContractInfoCollectionStatusDto> GetContractInfoCollectionStatusAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                SELECT ccs.ContractId,
                   ccs.FincoreStatusId,
                   ccs.CollectionStatusCode,
                   ccs.IsActive,
                   C2.FullName as ClientFullName,
                   C2.IdentityNumber as ClientIdentityNumber,
                   G.DisplayName as GroupDisplayName,
                   cps.DelayDays
            FROM CollectionContractStatuses ccs
                     OUTER APPLY (SELECT top 1 DATEDIFF(day, Date, dbo.GETASTANADATE()) AS DelayDays, *
                                  FROM ContractPaymentSchedule
                                  WHERE DeleteDate is null
                                    AND Canceled is null
                                    AND ActualDate is null
                                    AND ContractId = ccs.ContractId
                                  order by Date) cps
                    JOIN dbo.Contracts C on C.Id = ccs.ContractId
                    JOIN dbo.Groups G on G.Id = C.BranchId
                    Join dbo.Clients C2 on C2.Id = C.ClientId
            WHERE ccs.DeleteDate IS NULL
              AND ccs.ContractId = @ContractId
              AND C2.DeleteDate IS NULL
              AND cps.DeleteDate IS NULL";

            var result =
                await UnitOfWork.Session
                    .QueryFirstOrDefaultAsync<ContractInfoCollectionStatusDto>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public ContractInfoCollectionStatusDto GetContractInfoCollectionStatus(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                 SELECT ccs.ContractId,
                   ccs.FincoreStatusId,
                   ccs.CollectionStatusCode,
                   ccs.IsActive,
                   C2.FullName as ClientFullName,
                   C2.IdentityNumber as ClientIdentityNumber,
                   G.DisplayName as GroupDisplayName,
                   cps.DelayDays
            FROM CollectionContractStatuses ccs
                     OUTER APPLY (SELECT top 1 DATEDIFF(day, Date, dbo.GETASTANADATE()) AS DelayDays, *
                                  FROM ContractPaymentSchedule
                                  WHERE DeleteDate is null
                                    AND Canceled is null
                                    AND ActualDate is null
                                    AND ContractId = ccs.ContractId
                                  order by Date) cps
                    JOIN dbo.Contracts C on C.Id = ccs.ContractId
                    JOIN dbo.Groups G on G.Id = C.BranchId
                    Join dbo.Clients C2 on C2.Id = C.ClientId
            WHERE ccs.DeleteDate IS NULL
              AND ccs.ContractId = @ContractId
              AND C2.DeleteDate IS NULL
              AND cps.DeleteDate IS NULL";

            var result =
                UnitOfWork.Session
                    .QueryFirstOrDefault<ContractInfoCollectionStatusDto>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }

        public async Task<LegalCaseContractInfoDto> GetContractInfoAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };
            
            var sqlQuery = @"
                   SELECT 
                   C.ContractDate,
                   C.Id                                         as 'ContractId',
                   CL.Id                                        as 'ClientId',
                   CL.IdentityNumber                            as 'ClientIdentityNumber',
                   CL.FullName                                  as 'ClientFullName',
                   CL.Name                                      as 'ClientName',
                   CL.Surname                                   as 'ClientSurname',
                   CL.Patronymic                                as 'ClientPatronymic',
                   CR.Id                                        as 'CarId',
                   CR.TransportNumber                           as 'CarNumber',
                   CR.Mark                                      as 'CarMark',
                   CR.Model                                     as 'CarModel',
                   CR.BodyNumber								as 'BodyNumber',
				   CR.Color										as 'Color',
                   CR.ReleaseYear                               as 'ReleaseYear',
                   PS.Id                                        as 'PositionId',
                   PS.StatusName                                as 'PositionStatusName',
                   LPS.Id                                       as 'ProductId',
                   LPS.Name                                     as 'ProductName',
                   G.Id                                         as 'BranchId',
                   G.DisplayName                                as 'BranchName',
                   DATEDIFF(day, cps.Date, dbo.GETASTANADATE()) AS DelayDays
            FROM Contracts C
                     LEFT JOIN (SELECT ContractId,
                                       MIN(Date) AS EarliestPaymentDate
                                FROM ContractPaymentSchedule
                                WHERE DeleteDate IS NULL
                                  AND Canceled IS NULL
                                  AND ActualDate IS NULL
                                GROUP BY ContractId) AS minPayments ON C.Id = minPayments.ContractId
                     LEFT JOIN ContractPaymentSchedule cps ON C.Id = cps.ContractId AND minPayments.EarliestPaymentDate = cps.Date
                     LEFT JOIN Clients CL ON CL.Id = C.ClientId
                     LEFT JOIN ContractPositions CP ON C.Id = CP.ContractId
                     LEFT JOIN Cars CR ON CP.PositionId = CR.Id
                     LEFT JOIN ParkingStatuses PS ON CR.ParkingStatusId = PS.Id
                     LEFT JOIN LoanPercentSettings LPS ON C.SettingId = LPS.Id
                     LEFT JOIN Groups G ON C.BranchId = G.Id
                     LEFT JOIN LegalCaseContractsStatus LC ON C.Id = LC.ContractId
            WHERE C.DeleteDate IS NULL
              AND LC.ContractId IS NOT NULL
              AND C.Id = @ContractId
              AND CL.DeleteDate IS NULL
              AND PS.DeleteDate IS NULL
              AND LPS.DeleteDate IS NULL";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync(sqlQuery, parameters, UnitOfWork.Transaction);

            if (result is null || result.ContractId == null)
            {
                return null;
            }

            var product = result.ProductId != null && result.ProductName != null
                ? new ProductDto { Id = (int)result.ProductId, Name = result.ProductName }
                : null;

            var client = result.ClientId != null
                ? new ClientDto
                {
                    Id = result.ClientId,
                    IdentityNumber = result?.ClientIdentityNumber,
                    Name = result?.ClientName,
                    Surname = result?.ClientSurname,
                    Patronymic = result?.ClientPatronymic,
                    FullName = result?.ClientFullName
                }
                : null;

            var delayDays = await GetDelayDaysAsync(contractId);
            var legalCaseContractInfo = new LegalCaseContractInfoDto
            {
                DelayDays = delayDays,
                BranchId = result?.BranchId,
                BranchName = result?.BranchName,
                ContractDate = result?.ContractDate,
                Product = product,
                Client = client,
                Car = await GetCarByPositionAsync(contractId)
            };
            
            return legalCaseContractInfo;
        }

        public LegalCaseContractInfoDto GetContractInfo(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery3 = @"
                   SELECT 
                   C.ContractDate,
                   C.Id                                         as 'ContractId',
                   CL.Id                                        as 'ClientId',
                   CL.IdentityNumber                            as 'ClientIdentityNumber',
                   CL.FullName                                  as 'ClientFullName',
                   CL.Name                                      as 'ClientName',
                   CL.Surname                                   as 'ClientSurname',
                   CL.Patronymic                                as 'ClientPatronymic',
                   CR.Id                                        as 'CarId',
                   CR.TransportNumber                           as 'CarNumber',
                   CR.Mark                                      as 'CarMark',
                   CR.Model                                     as 'CarModel',
                   PS.Id                                        as 'PositionId',
                   PS.StatusName                                as 'PositionStatusName',
                   LPS.Id                                       as 'ProductId',
                   LPS.Name                                     as 'ProductName',
                   G.Id                                         as 'BranchId',
                   G.DisplayName                                as 'BranchName',
                   DATEDIFF(day, cps.Date, dbo.GETASTANADATE()) AS DelayDays
            FROM Contracts C
                     LEFT JOIN (SELECT ContractId,
                                       MIN(Date) AS EarliestPaymentDate
                                FROM ContractPaymentSchedule
                                WHERE DeleteDate IS NULL
                                  AND Canceled IS NULL
                                  AND ActualDate IS NULL
                                GROUP BY ContractId) AS minPayments ON C.Id = minPayments.ContractId
                     LEFT JOIN ContractPaymentSchedule cps ON C.Id = cps.ContractId AND minPayments.EarliestPaymentDate = cps.Date
                     LEFT JOIN Clients CL ON CL.Id = C.ClientId
                     LEFT JOIN ContractPositions CP ON C.Id = CP.ContractId
                     LEFT JOIN Cars CR ON CP.PositionId = CR.Id
                     LEFT JOIN ParkingStatuses PS ON CR.ParkingStatusId = PS.Id
                     LEFT JOIN LoanPercentSettings LPS ON C.SettingId = LPS.Id
                     LEFT JOIN Groups G ON C.BranchId = G.Id
                     LEFT JOIN LegalCaseContractsStatus LC ON C.Id = LC.ContractId
            WHERE C.DeleteDate IS NULL
              AND LC.ContractId IS NOT NULL
              AND C.Id = @ContractId
              AND CL.DeleteDate IS NULL
              AND PS.DeleteDate IS NULL
              AND LPS.DeleteDate IS NULL";

            var result = UnitOfWork.Session.QueryFirstOrDefault(sqlQuery3, parameters, UnitOfWork.Transaction);

            if (result is null || result.ContractId == null)
            {
                return null;
            }

            var product = result.ProductId != null && result.ProductName != null
                ? new ProductDto { Id = (int)result.ProductId, Name = result.ProductName }
                : null;

            var client = result.ClientId != null
                ? new ClientDto
                {
                    Id = result.ClientId,
                    IdentityNumber = result?.ClientIdentityNumber,
                    Name = result?.ClientName,
                    Surname = result?.ClientSurname,
                    Patronymic = result?.ClientPatronymic,
                    FullName = result?.ClientFullName
                }
                : null;

            var delayDays = GetDelayDays(contractId);
            var legalCaseContractInfo = new LegalCaseContractInfoDto
            {
                DelayDays = delayDays,
                BranchId = result?.BranchId,
                BranchName = result?.BranchName,
                ContractDate = result?.ContractDate,
                Product = product,
                Client = client,
                Car = GetCarByPosition(contractId)
            };
            
            return legalCaseContractInfo;
        }

        public async Task<ClientDto> GetClientByContractIdAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                SELECT CL.Id, CL.IdentityNumber, CL.Surname, Cl.Name, CL.Patronymic, CL.FullName
                FROM Clients CL
                         JOIN Contracts CNTR on CL.Id = CNTR.ClientId
                WHERE CNTR.DeleteDate IS NULL
                  AND CNTR.Id = @ContractId
                  AND CL.DeleteDate IS NULL";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<ClientDto>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }
        
        public ClientDto GetClientByContractId(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                SELECT CL.Id, CL.IdentityNumber, CL.Surname, Cl.Name, CL.Patronymic, CL.FullName
                FROM Clients CL
                         JOIN Contracts CNTR on CL.Id = CNTR.ClientId
                WHERE CNTR.DeleteDate IS NULL
                  AND CNTR.Id = @ContractId
                  AND CL.DeleteDate IS NULL";

            var result = UnitOfWork.Session.QueryFirstOrDefault<ClientDto>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }

        public async Task<int> GetPennyAccountIdByContractIdAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                SELECT a.Id
                FROM Accounts a
                         join Contracts c on a.ContractId = c.Id
                         join AccountSettings ass on ass.Id = a.AccountSettingId
                         join TypesHierarchy th on th.Id = ass.TypeId
                WHERE c.DeleteDate IS NULL
                  AND c.Id = @ContractId
                  AND ass.Code = 'PENY_ACCOUNT'
                  AND ass.DeleteDate IS NULL
                  AND th.DeleteDate IS NULL";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<int>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public int GetPennyAccountIdByContractId(int contractId)
        {
            var parameters = new { ContractId = contractId };
            
            var sqlQuery = @"
                SELECT a.Id
                FROM Accounts a
                         join Contracts c on a.ContractId = c.Id
                         join AccountSettings ass on ass.Id = a.AccountSettingId
                         join TypesHierarchy th on th.Id = ass.TypeId
                WHERE c.DeleteDate IS NULL
                  AND c.Id = @ContractId
                  AND ass.Code = 'PENY_ACCOUNT'
                  AND ass.DeleteDate IS NULL
                  AND th.DeleteDate IS NULL";

            var result = UnitOfWork.Session.QueryFirstOrDefault<int>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public async Task<int> GetPennyProfitIdByContractIdAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                select top 50 a.Id
                from Accounts a
                         join Contracts c on a.ContractId = c.Id
                    join AccountSettings ass on ass.Id = a.AccountSettingId
                         join TypesHierarchy th on th.Id = ass.TypeId
                where c.Id = @ContractId
                and ass.Code = 'PENY_PROFIT'";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<int>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public int GetPennyProfitIdByContractId(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                select top 50 a.Id
                from Accounts a
                         join Contracts c on a.ContractId = c.Id
                    join AccountSettings ass on ass.Id = a.AccountSettingId
                         join TypesHierarchy th on th.Id = ass.TypeId
                where c.Id = @ContractId
                and ass.Code = 'PENY_PROFIT'";

            var result = UnitOfWork.Session
                .QueryFirstOrDefault<int>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public async Task<CountResult<Contract>> GetContractsByFilterDataAsync(
            int page,
            int size,
            string contractNumber,
            string identityNumber,
            string fullName,
            string carNumber,
            string RKA,
            int? parkingStatusId,
            int? regionId,
            int? branchId,
            int? collateralType)
        {
           #region build predicate
            var pre = new StringBuilder(@"WHERE C.DeleteDate IS NULL");
            var preOr = new List<string>();

            if (contractNumber != null)
            {
                preOr.Add($"ContractNumber = @contractNumber");
            }

            if (identityNumber != null)
            {
                preOr.Add($"cl.IdentityNumber = @identityNumber");
            }

            if (fullName != null)
            {
                preOr.Add($"(CHARINDEX(@fullName, LOWER(CL.Name)) > 0 OR CHARINDEX(@fullName, LOWER(CL.Surname)) > 0)");
            }

            if (carNumber != null)
            {
                preOr.Add($"cr.TransportNumber = @carNumber");
            }

            if (RKA != null)
            {
                preOr.Add($"r.CadastralNumber = @RKA");
            }

            if (preOr.Any())
            {
                pre.Append($"\r\n AND ({string.Join("\r\n OR ", preOr.ToArray())})");
            }

            if (branchId.HasValue)
            {
                // pre.Append($"\r\n AND g.Id = @branchId");
                pre.Append($"\r\n AND c.BranchId = @BranchId or ob.SelectedBranchId = @BranchId");
            }
            
            if (regionId.HasValue)
            {
                pre.Append($"\r\n  AND g.RegionId = @regionId");
            }
            
            if (parkingStatusId.HasValue)
            {
                pre.Append($"\r\n AND cr.ParkingStatusId = @parkingStatusId");
            }
            
            if (collateralType.HasValue)
            {
                pre.Append($"\r\n AND C.CollateralType = @collateralType");
            }
            
            #endregion

            var baseQuery = $@"
                FROM Contracts C
                INNER JOIN LegalCaseContractsStatus lcs ON lcs.ContractId = C.Id
                LEFT JOIN Clients cl ON c.ClientId = cl.Id
                LEFT JOIN Groups g ON c.BranchId = g.Id
                LEFT JOIN ContractPositions cp ON c.Id = cp.ContractId
                LEFT JOIN Cars cr ON cp.PositionId = cr.Id
                LEFT JOIN ParkingStatuses ps ON cr.ParkingStatusId = ps.Id
                LEFT JOIN Realties r ON cp.PositionId = r.Id
                LEFT JOIN dogs.ContractAdditionalInfo OB on OB.Id = C.Id
                {pre}";
            
            var dataQuery = $@"
                SELECT C.*, g.RegionId
                {baseQuery}
                ORDER BY C.Id
                OFFSET @Page * @Size ROWS
                FETCH NEXT @Size ROWS ONLY";
            
            var getCountQuery = $@"
                SELECT COUNT(*) as TotalCount
                {baseQuery}";
            
            var query = $@"
                SELECT C.*, g.RegionId
                FROM Contracts C
                INNER JOIN LegalCaseContractsStatus lcs ON lcs.ContractId = C.Id
                LEFT JOIN Clients cl ON c.ClientId = cl.Id
                LEFT JOIN Groups g ON c.BranchId = g.Id
                LEFT JOIN ContractPositions cp ON c.Id = cp.ContractId
                LEFT JOIN Cars cr ON cp.PositionId = cr.Id
                LEFT JOIN ParkingStatuses ps ON cr.ParkingStatusId = ps.Id
                LEFT JOIN Realties r ON cp.PositionId = r.Id
                LEFT JOIN dogs.ContractAdditionalInfo OB on OB.Id = C.Id
                {pre}
                ORDER BY C.Id
                OFFSET @page * @size ROWS
                FETCH NEXT @size ROWS ONLY";

            var countQuery = $@"
                SELECT COUNT(*) as TotalCount
                FROM Contracts C
                INNER JOIN LegalCaseContractsStatus lcs ON lcs.ContractId = C.Id
                LEFT JOIN Clients cl ON c.ClientId = cl.Id
                LEFT JOIN Groups g ON c.BranchId = g.Id
                LEFT JOIN ContractPositions cp ON c.Id = cp.ContractId
                LEFT JOIN Cars cr ON cp.PositionId = cr.Id
                LEFT JOIN ParkingStatuses ps ON cr.ParkingStatusId = ps.Id
                LEFT JOIN Realties r ON cp.PositionId = r.Id
                LEFT JOIN dogs.ContractAdditionalInfo OB on OB.Id = C.Id
                {pre}";

            var result = await UnitOfWork.Session.QueryAsync<Contract>(dataQuery,
                new
                {
                    page,
                    size,
                    contractNumber,
                    identityNumber,
                    fullName,
                    carNumber,
                    RKA,
                    parkingStatusId,
                    branchId,
                    regionId,
                    collateralType
                }, UnitOfWork.Transaction);
            
            var count = await UnitOfWork.Session.QueryFirstOrDefaultAsync<int>(getCountQuery,
                new
                {
                    contractNumber,
                    identityNumber,
                    fullName,
                    carNumber,
                    RKA,
                    parkingStatusId,
                    branchId,
                    regionId,
                    collateralType
                }, UnitOfWork.Transaction);
            
            return new CountResult<Contract>(result, count);
        }

        public Task<List<ClientDto>> GetClientByFullNameAsync(string fullName)
        {
            throw new System.NotImplementedException();
        }

        public CountResult<Contract> GetContractsByFilterData(
            int page,
            int size,
            string? contractNumber,
            string? identityNumber,
            string? fullName,
            string? carNumber,
            string? RKA,
            int? parkingStatusId,
            int? branchId,
            int? regionId,
            int? collateralType)
        {
            #region build predicate
            var pre = new StringBuilder(@"WHERE C.DeleteDate IS NULL");
            var preOr = new List<string>();

            if (contractNumber != null)
            {
                preOr.Add($"ContractNumber = @contractNumber");
            }

            if (identityNumber != null)
            {
                preOr.Add($"cl.IdentityNumber = @identityNumber");
            }

            if (fullName != null)
            {
                preOr.Add($"(CHARINDEX(@fullName, LOWER(CL.Name)) > 0 OR CHARINDEX(@fullName, LOWER(CL.Surname)) > 0)");
            }

            if (carNumber != null)
            {
                preOr.Add($"cr.TransportNumber = @carNumber");
            }

            if (RKA != null)
            {
                preOr.Add($"r.CadastralNumber = @RKA");
            }

            if (preOr.Any())
            {
                pre.Append($"\r\n AND ({string.Join("\r\n OR ", preOr.ToArray())})");
            }

            if (branchId.HasValue)
            {
                pre.Append($"\r\n AND g.Id = @branchId");
            }
            
            if (regionId.HasValue)
            {
                pre.Append($"\r\n  AND g.RegionId = @regionId");
            }
            
            if (parkingStatusId.HasValue)
            {
                pre.Append($"\r\n AND cr.ParkingStatusId = @parkingStatusId");
            }
            
            if (collateralType.HasValue)
            {
                pre.Append($"\r\n AND C.CollateralType = @collateralType");
            }
            
            #endregion
            
            var query = $@"
                SELECT C.*, g.RegionId
                FROM Contracts C
                INNER JOIN LegalCaseContractsStatus lcs ON lcs.ContractId = C.Id
                LEFT JOIN Clients cl ON c.ClientId = cl.Id
                LEFT JOIN Groups g ON c.BranchId = g.Id
                LEFT JOIN ContractPositions cp ON c.Id = cp.ContractId
                LEFT JOIN Cars cr ON cp.PositionId = cr.Id
                LEFT JOIN ParkingStatuses ps ON cr.ParkingStatusId = ps.Id
                LEFT JOIN Realties r ON cp.PositionId = r.Id
                {pre}
                ORDER BY C.Id
                OFFSET @page * @size ROWS
                FETCH NEXT @size ROWS ONLY";
            
            var countQuery = $@"
                SELECT COUNT(*) as TotalCount
                FROM Contracts C
                INNER JOIN LegalCaseContractsStatus lcs ON lcs.ContractId = C.Id
                LEFT JOIN Clients cl ON c.ClientId = cl.Id
                LEFT JOIN Groups g ON c.BranchId = g.Id
                LEFT JOIN ContractPositions cp ON c.Id = cp.ContractId
                LEFT JOIN Cars cr ON cp.PositionId = cr.Id
                LEFT JOIN ParkingStatuses ps ON cr.ParkingStatusId = ps.Id
                LEFT JOIN Realties r ON cp.PositionId = r.Id
                {pre}";

            var result =  UnitOfWork.Session.Query<Contract>(query,
                new
                {
                    page,
                    size,
                    contractNumber,
                    identityNumber,
                    fullName,
                    carNumber,
                    RKA,
                    parkingStatusId,
                    branchId,
                    regionId,
                    collateralType
                });
            
            var count = UnitOfWork.Session.QueryFirstOrDefault<int>(countQuery,
                new
                {
                    contractNumber,
                    identityNumber,
                    fullName,
                    carNumber,
                    RKA,
                    parkingStatusId,
                    branchId,
                    regionId,
                    collateralType
                });
            
            return new CountResult<Contract>(result, count);
        }

        public async Task<List<Account>> GetAccountsByAccountSettingsAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };
            
            /// 55,56,57,58 - для кредитов
            /// 128,129,130,131 - для траншей
            var sqlQuery = @"
                SELECT  *
                FROM Accounts A
                    JOIN dbo.AccountSettings ACS on A.AccountSettingId = ACS.Id
                WHERE ACS.Id IN (55, 56, 57, 58, 128, 129, 130, 131)
                  AND ContractId = @ContractId
                ";

            var result = await UnitOfWork.Session
                .QueryAsync<Account>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result?.ToList() ?? new List<Account>();
        }

        public List<Account> GetAccountsByAccountSettings(int contractId)
        {
            var parameters = new { ContractId = contractId };
            
            /// 55,56,57,58 - для кредитов
            /// 128,129,130,131 - для траншей
            var sqlQuery = @"
                SELECT  *
                FROM Accounts A
                    JOIN dbo.AccountSettings ACS on A.AccountSettingId = ACS.Id
                WHERE ACS.Id IN (55, 56, 57, 58, 128, 129, 130, 131)
                  AND ContractId = @ContractId";
            
            var result = UnitOfWork.Session.Query<Account>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result?.ToList() ?? new List<Account>();
        }

        public async Task<int?> GetDelayDaysAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };
            
            var sqlQuery = @"
                SELECT DATEDIFF(Day,ccs.StartDelayDate,dbo.GetAstanaDate()) 
                FROM CollectionContractStatuses(nolock) ccs
                WHERE ccs.ContractId =  @ContractId";

            var delayDays = await UnitOfWork.Session
                .ExecuteScalarAsync<int?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            if (delayDays <= 0 || !delayDays.HasValue)
            {
                delayDays = 0;
            }

            return delayDays;
        }
        
        public int? GetDelayDays(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                SELECT DATEDIFF(Day,ccs.StartDelayDate,dbo.GetAstanaDate()) 
                FROM CollectionContractStatuses(nolock) ccs
                WHERE ccs.ContractId =  @ContractId";

            var delayDays = UnitOfWork.Session.ExecuteScalar<int?>(sqlQuery, parameters);

            return delayDays;
        }
        
        public async Task<LegalCasePrintTemplateModel> GetPrintFormDataAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };
            var sqlQuery = @"
                SELECT 
                    c.Id AS ContractId,
                    c.ContractNumber,
                    c.ContractDate,
                    c.LoanCost,
                    c.LoanPeriod,
                    c.LoanPercent,
                    c.APR as AnnualEffectiveRate,
                    o.configuration as OrganizationConfiguration,
                    cl.FullName,
                    ca.FullPathRus as FullAddress,
                    cc.Address as ContactPhoneNumber,
                    cl.IdentityNumber as IIN,
                    cl.BirthDay
                FROM Contracts c
                LEFT JOIN Members m ON c.ownerId = m.Id
                LEFT JOIN Organizations o ON o.id = m.organizationId
                INNER JOIN Clients cl ON cl.Id = c.ClientId
                INNER JOIN ClientAddresses ca ON ca.ClientId = cl.Id
                LEFT JOIN ClientContacts cc ON cl.id = cc.clientId
                WHERE c.DeleteDate IS NULL
                  AND cl.DeleteDate IS NULL
                  AND ca.DeleteDate IS NULL
                  AND (cc.DeleteDate IS NULL OR cc.DeleteDate IS NULL)
                  AND c.Id = @ContractId";

            LegalCasePrintTemplateModel result = null;

            var queryResult = await UnitOfWork.Session
                .QueryAsync<LegalCasePrintTemplateContractData, string, LegalCasePrintTemplateClientData,
                    LegalCasePrintTemplateModel>(
                    sqlQuery,
                    (contractData, organizationConfiguration, clientData) =>
                    {
                        if (result == null)
                        {
                            result = new LegalCasePrintTemplateModel
                            {
                                ContractData = contractData,
                                ClientData = clientData,
                                CompanyData = ExtractCompanyDataFromJson(organizationConfiguration)
                            };
                        }

                        return result;
                    },
                    parameters,
                    splitOn: "OrganizationConfiguration,FullName");

            return result;
        }
        
        public LegalCasePrintTemplateModel GetPrintFormData(int contractId)
        {
            var parameters = new { ContractId = contractId };
            var sqlQuery = @"
                SELECT 
                    c.Id AS ContractId,
                    c.ContractNumber,
                    c.ContractDate,
                    c.LoanCost,
                    c.LoanPeriod,
                    c.LoanPercent,
                    c.APR as AnnualEffectiveRate,
                    o.configuration as OrganizationConfiguration,
                    cl.FullName,
                    ca.FullPathRus as FullAddress,
                    cc.Address as ContactPhoneNumber,
                    cl.IdentityNumber as IIN,
                    cl.BirthDay
                FROM Contracts c
                LEFT JOIN Members m ON c.ownerId = m.Id
                LEFT JOIN Organizations o ON o.id = m.organizationId
                INNER JOIN Clients cl ON cl.Id = c.ClientId
                INNER JOIN ClientAddresses ca ON ca.ClientId = cl.Id
                LEFT JOIN ClientContacts cc ON cl.id = cc.clientId
                WHERE c.DeleteDate IS NULL
                  AND cl.DeleteDate IS NULL
                  AND ca.DeleteDate IS NULL
                  AND (cc.DeleteDate IS NULL OR cc.DeleteDate IS NULL)
                  AND c.Id = @ContractId";

            LegalCasePrintTemplateModel result = null;

            UnitOfWork.Session.Query<LegalCasePrintTemplateContractData, string, LegalCasePrintTemplateClientData, LegalCasePrintTemplateModel>(
                sqlQuery,
                (contractData, organizationConfiguration, clientData) =>
                {
                    if (result == null)
                    {
                        result = new LegalCasePrintTemplateModel
                        {
                            ContractData = contractData,
                            ClientData = clientData,
                            CompanyData = ExtractCompanyDataFromJson(organizationConfiguration)
                        };
                    }

                    return result;
                },
                parameters,
                splitOn: "OrganizationConfiguration,FullName");

            return result;
        }

        public async Task<IEnumerable<LegalCasePrintTemplateClientData>> GetPrintTemplateSubjectList(int contractId, string subjectCode)
        {
            var parameters = new { ContractId = contractId, SubjectCode = subjectCode };
            
            var sqlQuery = $@"
              SELECT
	            c.FullName, 
	            ca.FullPathRus as FullAddress, 
	            cc.Address as ContactPhoneNumber, 
	            c.IdentityNumber as IIN, 
	            c.BirthDay 
              FROM ContractLoanSubjects cls
              LEFT JOIN LoanSubjects ls on cls.SubjectId = ls.Id
              LEFT join Clients c ON c.Id = cls.ClientId
              LEFT join ClientAddresses ca ON ca.ClientId = cls.ClientId AND ca.AddressTypeId = 5
              LEFT join ClientContacts cc ON cc.ClientId = cls.ClientId AND cc.IsDefault = 1 AND cc.ContactTypeId = 1
	           WHERE ls.Code = @SubjectCode
                    AND cls.ContractId = @ContractId
		            AND cls.DeleteDate IS NULL
                    AND ca.DeleteDate IS NULL
		            AND cc.DeleteDate IS NULL";

            return await UnitOfWork.Session
                .QueryAsync<LegalCasePrintTemplateClientData>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<LegalCasePrintTemplateCarData>> GetPrintTemplateCarPositionList(int contractId)
        {
            var parameters = new { ContractId = contractId };
            
            var sqlQuery = $@"
              SELECT 
	            car.Mark
	            ,car.Model
	            ,car.ReleaseYear
	            ,car.TransportNumber
	            ,car.BodyNumber
            FROM Cars car
            inner join ContractPositions cp on cp.PositionId = car.Id
            WHERE cp.ContractId = @ContractId";

            return await UnitOfWork.Session
                .QueryAsync<LegalCasePrintTemplateCarData>(sqlQuery, parameters, UnitOfWork.Transaction);
        }
        
        private async Task<CarDto> GetCarByPositionAsync(int contractId)
        {
            var contract = await GetContract(contractId);
            if (contract.ContractClass == ContractClass.Tranche)
            {
                contractId = await GetTrancheCreditLineId(contractId);
            }
            
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                SELECT CR.Id,
                       CR.TransportNumber,
                       CR.Mark,
                       CR.Model,
                       CR.BodyNumber,
                       CR.Color,
                       CR.ReleaseYear,
                       PS.StatusName,
                       PS.Id as 'StatusId'
                FROM Contracts C
                         LEFT JOIN dbo.ContractPositions P ON P.ContractId = C.Id
                         LEFT JOIN Cars CR ON P.PositionId = CR.Id AND CR.Id IS NOT NULL
                         LEFT JOIN ParkingStatuses PS ON CR.ParkingStatusId = PS.Id
                WHERE CR.Id IS NOT NULL
                  AND CR.TransportNumber IS NOT NULL
                  AND C.Id = @ContractId";

            var car = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<CarDto>(sqlQuery, parameters, UnitOfWork.Transaction);
            return car;
        }

        private async Task<Contract> GetContract(int contractId)
        {
            var parameters = new { ContractId = contractId };
            var sqlQuery = "SELECT * FROM Contracts WHERE Id = @ContractId";
            
            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<Contract>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        private async Task<int> GetTrancheCreditLineId(int trancheId)
        {
            var parameters = new { Id = trancheId };
            var sqlQuery = "SELECT CreditLineId FROM dogs.Tranches WHERE Id = @Id";
            
            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<int>(sqlQuery, parameters, UnitOfWork.Transaction);
        }
        
        private CarDto GetCarByPosition(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                SELECT CR.Id,
                       CR.TransportNumber,
                       CR.Mark,
                       CR.Model,
                       PS.StatusName,
                       PS.Id as 'StatusId'
                FROM Contracts C
                     LEFT JOIN dbo.Positions P ON P.ClientId = C.ClientId
                     LEFT JOIN dbo.Clients CL ON CL.Id = C.ClientId
                     LEFT JOIN Cars CR ON P.Id = CR.Id AND CR.Id IS NOT NULL
                     LEFT JOIN ParkingStatuses PS ON CR.ParkingStatusId = PS.Id
                WHERE CR.Id IS NOT NULL
                AND CR.TransportNumber IS NOT NULL
                AND C.Id = @ContractId";

            var car = UnitOfWork.Session.QueryFirstOrDefault<CarDto>(sqlQuery, parameters, UnitOfWork.Transaction);
            return car;
        }
        
        private LegalCasePrintTemplateCompanyData ExtractCompanyDataFromJson(string json)
        {
            var obj = JObject.Parse(json);
            if (obj == null)
                return null;
            
            var companyData = new LegalCasePrintTemplateCompanyData
            {
                FullName = obj["LegalSettings"]["LegalName"] != null ? obj["LegalSettings"]["LegalName"].ToString() : string.Empty,
                BIN = obj["LegalSettings"]["BIN"] != null ? obj["LegalSettings"]["BIN"].ToString() : string.Empty,
                IIK = obj["BankSettings"]["BankAccount"] != null ? obj["BankSettings"]["BankAccount"].ToString() : string.Empty,
                BIK = obj["BankSettings"]["BankBik"] != null ? obj["BankSettings"]["BankBik"].ToString() : string.Empty,
                BankFullName = obj["BankSettings"]["BankName"] != null ? obj["BankSettings"]["BankName"].ToString() : string.Empty,
                Address = obj["ContactSettings"]["Address"] != null ? obj["ContactSettings"]["Address"].ToString() : string.Empty
            };

            return companyData;
        }
    }
}