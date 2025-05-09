using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using Pawnshop.Data.Models.ApplicationOnlineNpck;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.ApplicationOnlineFileStorage;
using Pawnshop.Services.PDF;
using Pawnshop.Services.TasCore;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System;

namespace Pawnshop.Services.ApplicationOnlineFiles
{
    public sealed class ApplicationOnlineFilesService : IApplicationOnlineFilesService
    {
        private readonly ApplicationOnlineFileCodesRepository _applicationOnlineFileCodesRepository;
        private readonly ApplicationOnlineFileRepository _applicationOnlineFileRepository;
        private readonly ApplicationOnlineNpckFileRepository _applicationOnlineNpckFileRepository;
        private readonly ContractAdditionalInfoRepository _contractAdditionalInfoRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger _logger;
        private readonly IPdfService _pdfService;
        private readonly ITasCoreNpckService _tasCoreNpckService;

        private readonly string _signableLink;

        public ApplicationOnlineFilesService(
            ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            ApplicationOnlineFileRepository applicationOnlineFileRepository,
            ApplicationOnlineNpckFileRepository applicationOnlineNpckFileRepository,
            ContractAdditionalInfoRepository contractAdditionalInfoRepository,
            IFileStorageService fileStorageService,
            ILogger logger,
            IPdfService pdfService,
            ITasCoreNpckService tasCoreNpckService,
            IOptions<FileStorageOptions> options)
        {
            _applicationOnlineFileCodesRepository = applicationOnlineFileCodesRepository;
            _applicationOnlineFileRepository = applicationOnlineFileRepository;
            _applicationOnlineNpckFileRepository = applicationOnlineNpckFileRepository;
            _contractAdditionalInfoRepository = contractAdditionalInfoRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
            _pdfService = pdfService;
            _tasCoreNpckService = tasCoreNpckService;
            _signableLink = options.Value.SignableLink;
        }


        public bool CreateLoanContractFile(ApplicationOnline applicationOnline, out string error, Func<string> originalUrlBuilder,
            Guid fileId, string language = null, CancellationToken cancellationToken = default, bool sendToNpck = false)
        {
            error = string.Empty;

            try
            {
                var loanContractCode = _applicationOnlineFileCodesRepository
                    .GetApplicationOnlineFileCodeByBusinessType(Constants.APPLICATION_ONLINE_FILE_BUSINESS_TYPE_LOAN_CONTRACT, language);

                if (sendToNpck)
                {
                    var files = _applicationOnlineFileRepository.GetList(applicationOnline.Id, new List<Guid> { loanContractCode.Id });

                    foreach (var file in files)
                    {
                        file.Delete();
                        _applicationOnlineFileRepository.Update(file).Wait(cancellationToken);
                    }
                }

                var fileBytes = _pdfService.GetFile(applicationOnline.ContractId.Value, applicationOnline.CreditLineId, applicationOnline.SignType, cancellationToken).Result;

                if (fileBytes == null)
                    throw new Exception($"Не удалось получить файл договора!");

                using (var stream = new MemoryStream(fileBytes))
                {
                    var response = _fileStorageService.Upload(applicationOnline.ListId.ToString(), stream, String.Empty,
                        loanContractCode.BusinessType, $"{loanContractCode.Title}.pdf", cancellationToken).Result;

                    var fileStorageId = Guid.Parse(response.FileGuid);

                    var fileInfo = new ApplicationOnlineFile(fileId, applicationOnline.Id,
                        fileStorageId, "", "application/pdf", "", originalUrlBuilder, loanContractCode.Id);

                    _applicationOnlineFileRepository.Insert(fileInfo).Wait(cancellationToken);

                    if (sendToNpck)
                    {
                        var fileName = string.IsNullOrEmpty(loanContractCode.NpckTitle) ? loanContractCode.Title : loanContractCode.NpckTitle;
                        var additionalInfo = _contractAdditionalInfoRepository.Get(applicationOnline.ContractId.Value);
                        var npckResult = _tasCoreNpckService.DocumentUpload(fileStorageId, $"{_signableLink}{fileStorageId}", fileName, additionalInfo.LoanStorageFileId, cancellationToken).Result;

                        if (npckResult.Success)
                            _applicationOnlineNpckFileRepository.Insert(new ApplicationOnlineNpckFile(fileInfo.Id, npckResult.NpckFileId.Value, additionalInfo.LoanStorageFileId));
                        else
                            throw new Exception($"Error upload file to NPCK: {npckResult.Message}");
                    }
                    else
                    {
                        var additionalInfo = _contractAdditionalInfoRepository.Get(applicationOnline.ContractId.Value);

                        if (additionalInfo == null || string.IsNullOrEmpty(additionalInfo.SmsCode))
                            return true;

                        additionalInfo.LoanStorageFileId = fileStorageId;

                        if (!additionalInfo.StorageListId.HasValue)
                            additionalInfo.StorageListId = applicationOnline.ListId;

                        _contractAdditionalInfoRepository.Update(additionalInfo);
                    }

                    return true;
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                error = exception.Message;
                return false;
            }
        }
    }
}
