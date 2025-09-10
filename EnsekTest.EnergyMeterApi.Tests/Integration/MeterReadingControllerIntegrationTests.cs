using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using EnergyMeterApi.Models;


namespace EnsekTest.EnergyMeterApi.Tests.Integration
{
    public class MeterReadingControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        static MeterReadingControllerIntegrationTests()
        {
            SQLitePCL.Batteries_V2.Init();
        }

        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public MeterReadingControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> PostCsvAsync(string csv)
        {
            var form = new MultipartFormDataContent();
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(csv));
            content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            form.Add(content, "file", "meterreadings.csv");
            return await _client.PostAsync("/api/meterreading/meter-reading-uploads", form);
        }

        [Fact]
        public async Task PostMeterReading_ValidReading_ReturnsSuccess()
        {
            var csv = "AccountId,MeterReadingDateTime,MeterReadValue\n1234,01/01/2024 10:00,54321";
            var response = await PostCsvAsync(csv.Replace("\\n", "\n"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MeterReadingUploadResult>();
            Assert.NotNull(result);
            Assert.Equal(1, result.SuccessfulReadings);
            Assert.Equal(0, result.FailedReadings);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task PostMeterReading_DuplicateReading_ReturnsBadRequestWithErrors()
        {
            var csv = "AccountId,MeterReadingDateTime,MeterReadValue\n1234,01/01/2024 10:00,54321";
            await PostCsvAsync(csv.Replace("\\n", "\n"));
            var response = await PostCsvAsync(csv.Replace("\\n", "\n"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MeterReadingUploadResult>();
            Assert.NotNull(result);
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(1, result.FailedReadings);
            Assert.Contains(result.Errors, e => e.Contains("Duplicate reading"));
        }

        [Fact]
        public async Task PostMeterReading_InvalidMeterValue_ReturnsBadRequestWithErrors()
        {
            var csv = "AccountId,MeterReadingDateTime,MeterReadValue\n1234,01/01/2024 10:00,ABCDE";
            var response = await PostCsvAsync(csv.Replace("\\n", "\n"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MeterReadingUploadResult>();
            Assert.NotNull(result);
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(1, result.FailedReadings);
            Assert.Contains(result.Errors, e => e.Contains("Invalid meter reading value"));
        }

        [Fact]
        public async Task PostMeterReading_AccountDoesNotExist_ReturnsBadRequestWithErrors()
        {
            var csv = "AccountId,MeterReadingDateTime,MeterReadValue\n9999,01/01/2024 10:00,12345";
            var response = await PostCsvAsync(csv.Replace("\\n", "\n"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MeterReadingUploadResult>();
            Assert.NotNull(result);
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(1, result.FailedReadings);
            Assert.Contains(result.Errors, e => e.Contains("AccountId 9999 does not exist"));
        }

        [Fact]
        public async Task PostMeterReading_FutureDate_ReturnsBadRequestWithErrors()
        {
            var futureDate = DateTime.UtcNow.AddDays(1).ToString("dd/MM/yyyy HH:mm");
            var csv = $"AccountId,MeterReadingDateTime,MeterReadValue\n1234,{futureDate},12345";
            var response = await PostCsvAsync(csv.Replace("\\n", "\n"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MeterReadingUploadResult>();
            Assert.NotNull(result);
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(1, result.FailedReadings);
            Assert.Contains(result.Errors, e => e.Contains("date cannot be in the future"));
        }

        [Fact]
        public async Task PostMeterReading_NoFile_ReturnsBadRequest()
        {
            var form = new MultipartFormDataContent();
            var response = await _client.PostAsync("/api/meterreading/meter-reading-uploads", form);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string rawContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Failed to read the request form", rawContent);
            Assert.Contains("validation errors occurred", rawContent);
        }

        [Fact]
        public async Task PostMeterReading_NonCsvFile_ReturnsBadRequest()
        {
            var form = new MultipartFormDataContent();
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes("some content"));
            content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            form.Add(content, "file", "test.txt");
            var response = await _client.PostAsync("/api/meterreading/meter-reading-uploads", form);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MeterReadingUploadResult>();
            Assert.NotNull(result);
            Assert.Contains(result.Errors, e => e.Contains("File must be a CSV file"));
        }
    }
}