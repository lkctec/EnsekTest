using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EnergyMeterApi.Models;
using EnsekTest.EnergyMeterApi.Services;

namespace EnsekTest.EnergyMeterApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeterReadingController : ControllerBase
    {
        private readonly IRestApiServices _restApiService;

        public MeterReadingController(IRestApiServices restApiService)
        {
            _restApiService = restApiService;
        }

        [HttpPost("meter-reading-uploads")]
        public async Task<ActionResult<MeterReadingUploadResult>> UploadMeterReadings(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                var errorResult = new MeterReadingUploadResult();
                errorResult.Errors.Add("No file provided or file is empty");
                return BadRequest(errorResult);
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                var errorResult = new MeterReadingUploadResult();
                errorResult.Errors.Add("File must be a CSV file");
                return BadRequest(errorResult);
            }

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _restApiService.ProcessMeterReadingsCsvAsync(stream);
                
                if (result.Errors.Any() && result.SuccessfulReadings == 0)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorResult = new MeterReadingUploadResult();
                errorResult.Errors.Add($"Unexpected error: {ex.Message}");
                return StatusCode(500, errorResult);
            }
        }
    }
}
