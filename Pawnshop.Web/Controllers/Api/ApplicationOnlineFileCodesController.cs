using System;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Pawnshop.Data.Models.ApplicationOnlineFileCodes;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationOnlineFileCodesController : Controller
    {

        private readonly IMemoryCache _memoryCache;

        public ApplicationOnlineFileCodesController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet("filecodes")]
        [ProducesResponseType( 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFileCodes(
            [FromServices] ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            CancellationToken cancellationToken)
        {
            if (!_memoryCache.TryGetValue("fileCodes", out List<ApplicationOnlineFileCode> fileCodes))
            {
                fileCodes = applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodes();
                _memoryCache.Set("fileCodes", fileCodes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(fileCodes);
        }

        private string DocumentTypeFromDrppFormatToOurConverter(string drppType)
        {
            drppType = drppType.ToLowerInvariant();
            string result = "";

            var words = drppType.Split("_", StringSplitOptions.None);

            for (int i = 0; i < words.Length; i++)
            {
                char symbol = Char.ToUpperInvariant(words[i][0]);
                words[i] = words[i].Remove(0, 1).Insert(0, symbol.ToString());
                result += words[i];
            }


            return result;
        }
    }

    public sealed class FileCodeDrppJson
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }
}
