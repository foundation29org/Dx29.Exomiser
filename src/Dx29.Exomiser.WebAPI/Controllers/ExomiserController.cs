using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Dx29.Models;
using Dx29.Services;

namespace Dx29.Exomiser.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class ExomiserController : ControllerBase
    {
        const long SIZE_LIMIT = 500 * 1024 * 1024; // 500Mb Gb

        public ExomiserController(ExomiserClient exomiserClient)
        {
            ExomiserClient = exomiserClient;
        }

        public ExomiserClient ExomiserClient { get; }

        [HttpPut("Process")]
        [RequestSizeLimit(SIZE_LIMIT)]
        public async Task<IActionResult> ProcessAsync([FromBody] ExomiserRequest request)
        {
            try
            {
                (var vcfSource, var vcfExtension) = GetVcfAssets(request, out string errorMessage);
                if (errorMessage == null)
                {
                    var pedSource = GetPedAssets(request, out errorMessage);
                    if (errorMessage == null)
                    {
                        var jobInfo = await ExomiserClient.PrepareJobAsync(request, vcfSource, vcfExtension, pedSource);
                        var status = await ExomiserClient.SendMessageAsync(jobInfo);

                        return Ok(status);
                    }
                }
                return BadRequest(errorMessage);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Process")]
        [RequestSizeLimit(SIZE_LIMIT)]
        [RequestFormLimits(MultipartBodyLengthLimit = SIZE_LIMIT)]
        public async Task<IActionResult> ProcessAsync(IFormFile requestInput, IList<IFormFile> files)
        {
            try
            {
                var request = GetExomiserRequest(requestInput);

                var jobInfo = await ExomiserClient.PrepareJobAsync(request, files);
                var status = await ExomiserClient.SendMessageAsync(jobInfo);

                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Status")]
        public async Task<IActionResult> Status([FromQuery] TokenParam parms)
        {
            try
            {
                var status = await ExomiserClient.GetStatusAsync(parms.Token);
                if (status != null)
                {
                    return Ok(status);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Results")]
        public async Task<IActionResult> Results([FromQuery] GetResultParam parms)
        {
            try
            {
                var status = await ExomiserClient.GetStatusAsync(parms.Token);
                if (status != null)
                {
                    if (status.Status == "Succeeded")
                    {
                        var output = await ExomiserClient.DownloadOutputStringAsync(parms.Token, parms.Filename);
                        return Content(output, "application/json");
                    }
                    return BadRequest("Invalid status.");
                }
                return StatusCode(404);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Notification")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Callback([FromBody] ExomiserNotification notification)
        {
            try
            {
                await Task.CompletedTask;
                Console.WriteLine(notification.Serialize());
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private ExomiserRequest GetExomiserRequest(IFormFile requestFile)
        {
            using (var reader = new StreamReader(requestFile.OpenReadStream()))
            {
                return reader.ReadToEnd().Deserialize<ExomiserRequest>();
            }
        }

        private (string, string) GetVcfAssets(ExomiserRequest request, out string errorMessage)
        {
            errorMessage = null;
            string vcfSource = Request.Headers["dx-source-vcf"];
            if (!String.IsNullOrEmpty(vcfSource))
            {
                if (IsValidUrl(vcfSource))
                {
                    string vcfExtension = FilenameHelper.GetExtension(request.VcfFilename);
                    if (vcfExtension == ".vcf" || vcfExtension == ".vcf.gz")
                    {
                        return (vcfSource, vcfExtension);
                    }
                    else
                    {
                        errorMessage = "Invalid vcf extension";
                    }
                }
                else
                {
                    errorMessage = "Invalid dx-source-vcf url";
                }
            }
            else
            {
                errorMessage = "Missing dx-source-vcf header";
            }
            return (null, null);
        }

        private string GetPedAssets(ExomiserRequest request, out string errorMessage)
        {
            errorMessage = null;
            string pedSource = Request.Headers["dx-source-ped"];
            if (String.IsNullOrEmpty(pedSource) && String.IsNullOrEmpty(request.PedFilename))
            {
                return null;
            }
            if (!String.IsNullOrEmpty(pedSource) && !String.IsNullOrEmpty(request.PedFilename))
            {
                return pedSource;
            }
            if (String.IsNullOrEmpty(pedSource))
            {
                errorMessage = "Missing dx-source-ped header";
            }
            else
            {
                errorMessage = "Missing ped filename";
            }
            return null;
        }

        private bool IsValidUrl(string url) => Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri validatedUri);
    }
}
