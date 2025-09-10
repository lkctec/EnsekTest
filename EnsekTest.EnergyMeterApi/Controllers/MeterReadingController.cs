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

        /// <summary>
        /// Uploads and processes a CSV file containing meter readings.
        /// </summary>
        /// <param name="file">The CSV file containing meter readings with columns: AccountId, MeterReadingDateTime, MeterReadValue.</param>
        /// <returns>
        /// A result object containing the count of successful and failed readings, along with any error messages.
        /// Returns 200 OK with results, 400 Bad Request for validation errors, or 500 Internal Server Error for unexpected errors.
        /// </returns>
        /// <response code="200">Successfully processed the CSV file (may include partial failures).</response>
        /// <response code="400">Invalid file format, missing file, or all readings failed validation.</response>
        /// <response code="500">An unexpected server error occurred during processing.</response>
        [HttpPost("meter-reading-uploads")]
        public async Task<ActionResult<MeterReadingUploadResult>> UploadMeterReadings(IFormFile file)
        {
            var errorResult = new MeterReadingUploadResult();
            if (file == null || file.Length == 0)
            {
                
                errorResult.Errors.Add("No file provided or file is empty");
                return BadRequest(errorResult);
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
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
                errorResult.Errors.Add($"Unexpected error: {ex.Message}");
                return StatusCode(500, errorResult);
            }
        }
    }
}
