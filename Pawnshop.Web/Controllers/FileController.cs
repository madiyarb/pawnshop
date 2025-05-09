using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Files;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Security;
using Pawnshop.Services.Storage;

namespace Pawnshop.Web.Controllers
{
    public class FileController : Controller
    {
        private readonly IStorage _storage;
        private readonly ISessionContext _sessionContext;
        private readonly TokenProvider _tokenProvider;
        private readonly FileRepository _fileRepository;
        private readonly EventLog _eventLog;

        public FileController(IStorage storage, ISessionContext sessionContext, TokenProvider tokenProvider, FileRepository fileRepository, EventLog eventLog)
        {
            _storage = storage;
            _sessionContext = sessionContext;
            _tokenProvider = tokenProvider;
            _fileRepository = fileRepository;
            _eventLog = eventLog;
        }

        [HttpGet("/file/{id}"), AllowAnonymous]
        public async Task<IActionResult> Download(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var tokenString = Request.Headers["Authorization"];

            tokenString = tokenString.ToString().Replace("Bearer ", "");
            var token = _tokenProvider.ReadToken(tokenString);
            _sessionContext.InitFromClaims(token.Claims.ToArray());

            var fileRow = _fileRepository.Get(id);
            if (fileRow == null)
            {
                return NotFound();
            }

            var stream = await _storage.Load(fileRow.FilePath);
            return File(stream, fileRow.ContentType, fileRow.FileName);
        }

        [HttpGet("/file/preview/{id}"), AllowAnonymous]
        public async Task<IActionResult> GetPath(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var tokenString = Request.Headers["Authorization"];
            var token = _tokenProvider.ReadToken(tokenString);
            _sessionContext.InitFromClaims(token.Claims.ToArray());

            var fileRow = _fileRepository.Get(id);
            if (fileRow == null)
            {
                return NotFound();
            }

            var stream = await _storage.Load(fileRow.FilePath);

            var filename = fileRow.FileName;
            filename = String.Join(
                "/",
                filename.Split("/").Select(s => System.Net.WebUtility.UrlEncode(s))
            );

            System.Net.Mime.ContentDisposition contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = true
            };

            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
            return File(stream, fileRow.ContentType);
        }

        [HttpGet("/file/temp/{name}"), AllowAnonymous]
        public async Task<IActionResult> DownloadTemp(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var stream = await _storage.Load(name, ContainerName.Temp);

            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(name, out contentType);
            return File(stream, contentType ?? "application/octet-stream", name);
        }

        [HttpPost("/file/"), Authorize]
        public async Task<IActionResult> Upload()
        {
            var files = Request.Form?.Files;
            if (files == null || files.Count == 0)
            {
                return NoContent();
            }

            var fileRows = new FileRow[files.Count];

            using (var transaction = _fileRepository.BeginTransaction())
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var fileRow = new FileRow
                    {
                        CreateDate = DateTime.Now,
                        ContentType = file.ContentType,
                        FileName = Path.GetFileName(file.FileName),
                        FilePath = await _storage.Save(file.OpenReadStream())
                    };
                    _fileRepository.Insert(fileRow);
                    _eventLog.Log(EventCode.FileUploaded, EventStatus.Success, null, null, fileRow.FileName, fileRow.FilePath);
                    fileRows[i] = fileRow;
                }

                transaction.Commit();
            }

            return Ok(fileRows);
        }

        [HttpGet("/file/{folder}/{fileName}"), AllowAnonymous]
        public async Task<IActionResult> DownloadByName(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentOutOfRangeException(nameof(fileName));
            var stream = await _storage.Load(fileName, Enum.Parse<ContainerName>(folder));
            FileInfo fi = new FileInfo(fileName);
            string mime = "";
            switch(fi.Extension.ToLower())
            {
                case ".pdf": mime = "application/pdf"; break;
                case ".xml": mime = "text/xml"; break;
                default: mime = "application/octet-stream"; break;
            }
            return File(stream, mime, fileName);
        }
    }
}