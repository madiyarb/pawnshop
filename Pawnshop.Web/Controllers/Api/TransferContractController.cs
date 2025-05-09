using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Transfers;
using Pawnshop.Data.Models.Transfers.TransferContracts;
using Pawnshop.Web.Engine;
using Pawnshop.Services.Storage;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Web.Engine.Security;
using Pawnshop.Web.Models.List;
using Pawnshop.Core.Exceptions;
using OfficeOpenXml;

namespace Pawnshop.Web.Controllers.Api
{
    public class TransferContractController : Controller
    {
        private readonly ContractRepository _repository;
        private readonly TransferContractRepository _transferContractRepository;
        private readonly TransferRepository _transferRepository;
        private readonly IStorage _storage;
        private readonly ISessionContext _sessionContext;
        private readonly TransferContractsExcelBuilder _excelBuilder;
        private readonly TokenProvider _tokenProvider;

        public TransferContractController(ISessionContext sessionContext, ContractRepository repository, TransferContractRepository transferContractRepository, 
            TransferRepository transferRepository,IStorage storage, TransferContractsExcelBuilder excelBuilder, TokenProvider tokenProvider)
        {
            _transferContractRepository = transferContractRepository;
            _transferRepository = transferRepository;
            _repository = repository;
            _storage = storage;
            _sessionContext = sessionContext;
            _excelBuilder = excelBuilder;
            _tokenProvider = tokenProvider;
        }

        [HttpPost]
        public int Upload(IFormCollection form, [FromQuery(Name = "token")] string tokenString)
        {
            if (tokenString == null) throw new ArgumentNullException(nameof(tokenString));

            var token = _tokenProvider.ReadToken(tokenString);
            _sessionContext.InitFromClaims(token.Claims.ToArray());

            Dictionary<string,string> contractNumbers = new Dictionary<string, string>();

            foreach (var file in form.Files)
            {
                using (var stream = file.OpenReadStream())
                using (var memoryStream = new MemoryStream())
                {
                    var package = new ExcelPackage(stream);
                    try
                    {
                        ExcelWorksheet workSheet = package.Workbook.Worksheets[1];
                        var j = 1;
                        for (int i = workSheet.Dimension.Start.Row;
                            i <= workSheet.Dimension.End.Row;
                            i++)

                        {
                            var contractNumber = workSheet.Cells[i, j].Value;
                            var clientIdentityNumber = workSheet.Cells[i, j+1].Value;

                            if (contractNumber != null || clientIdentityNumber != null) contractNumbers.Add($"\'{contractNumber.ToString()}\'", $"\'{clientIdentityNumber.ToString()}\'");
                        }

                        Int64 unixTimestamp = (Int64)DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
                        var fileName = _storage.Save(stream, ContainerName.ExcelImport,
                            $"{unixTimestamp}ExcelImport.docx");
                    }
                    catch (Exception e)
                    {
                        throw new PawnshopApplicationException($"Проверьте файл ({e.Message})");
                    }
                }
                
                int transferId = 0;
                int position = 1;
                List<Contract> contracts = _transferContractRepository.GetContractsForTransfer(contractNumbers);
                contracts = contracts.Where(x => x != null).ToList();
                using (var transaction = _transferRepository.BeginTransaction())
                {
                    Transfer transfer = new Transfer(){ UserId = _sessionContext.UserId };
                    _transferRepository.Insert(transfer);

                    transaction.Commit();
                    transferId = transfer.Id;
                }
                
                foreach (var cNumber in contractNumbers)
                {
                    var number = cNumber.Key.Replace("'", "");
                    var identityNumber = cNumber.Value.Replace("'", "");
                    var contract = contracts.Where(x => x.ContractNumber.Contains(number)).FirstOrDefault();

                    if (contract != null)
                    {
                        using (var transaction = _transferContractRepository.BeginTransaction())
                        {
                            TransferContract transferContract = new TransferContract();
                            transferContract.PoolTransferId = transferId;
                            transferContract.ContractId = contract.Id;
                            transferContract.EntryPosition = position;
                            transferContract.EntryСontractNumber = number;
                            transferContract.EntryСlientIdentityNumber = identityNumber;

                            _transferContractRepository.Insert(transferContract);

                            transaction.Commit();
                        }
                    }
                    else
                    {
                        using (var transaction = _transferContractRepository.BeginTransaction())
                        {
                            TransferContract transferContract = new TransferContract();
                            transferContract.PoolTransferId = transferId;
                            transferContract.ContractId = null;
                            transferContract.EntryPosition = position;
                            transferContract.EntryСontractNumber = number;
                            transferContract.EntryСlientIdentityNumber = identityNumber;

                            _transferContractRepository.Insert(transferContract);

                            transaction.Commit();
                        }
                    }
                    position++;
                }
                return transferId;
            }
            return 0;
        }
        
        [HttpPost]
        [Authorize(Permissions.ContractTransfer)]
        public ListModel<TransferContract> Card([FromBody] ListQuery listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var contracts = _transferContractRepository.List(listQuery);

            foreach (var contract in contracts)
            {
                if (contract.Contract != null) contract.Contract = _repository.Get(contract.Contract.Id);
            }

            return new ListModel<TransferContract>
            {
                List = contracts,
                Count = _transferContractRepository.Count(listQuery)
            };
        }

        [HttpPost]
        [Authorize(Permissions.ContractTransfer)]
        public TransferContract Save([FromBody] TransferContract transfer)
        {
            using (var transaction = _transferContractRepository.BeginTransaction())
            {
                if (transfer.Id > 0)
                {
                    transfer.Status = transfer.ActionId > 0 ? TransferContractStatus.Success : TransferContractStatus.Fail;
                    _transferContractRepository.Update(transfer);
                }
                else
                {
                    _transferContractRepository.Insert(transfer);
                }
                transaction.Commit();
            }
            return transfer;
        }

        [HttpPost]
        [Authorize(Permissions.ContractTransfer)]
        public List<TransferContract> MultiSave([FromBody] List<TransferContract> transfers)
        {
            foreach (var transfer in transfers)
            {
                using (var transaction = _transferContractRepository.BeginTransaction())
                {
                    if (transfer.Id > 0)
                    {
                        transfer.Status = transfer.ActionId > 0 ? TransferContractStatus.Success : TransferContractStatus.Fail;
                        _transferContractRepository.Update(transfer);
                    }
                    else
                    {
                        _transferContractRepository.Insert(transfer);
                    }
                    transaction.Commit();
                }
            }
            return transfers;
        }

        [HttpPost]
        [Authorize(Permissions.ContractTransfer)]
        public async Task<IActionResult> Export([FromBody] List<TransferContract> contracts)
        {
            using (var stream = _excelBuilder.Build(contracts))
            {
                var fileName = await _storage.Save(stream, ContainerName.Temp, "export.xlsx");
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);

                var fileRow = new FileRow
                {
                    CreateDate = DateTime.Now,
                    ContentType = contentType ?? "application/octet-stream",
                    FileName = fileName,
                    FilePath = fileName
                };
                return Ok(fileRow);
            }
        }
    }
    
}
