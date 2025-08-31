using EnergyMeterApi.Models;
using EnsekTest.EnergyMeterApi.Helpers;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace EnsekTest.EnergyMeterApi.Tests.Services
{
    public class CriticalApiTests : RestApiServiceTestBase
    {
        private static readonly CsvConfiguration _csvConfig = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        public CriticalApiTests()
        {
            _context.MeterReadings.AddRange(
                new MeterReading { AccountId = 1234, MeterReadingDateTime = new DateTime(2024, 1, 1, 10, 0, 0), MeterReadValue = "11111" },
                new MeterReading { AccountId = 5678, MeterReadingDateTime = new DateTime(2024, 1, 1, 11, 0, 0), MeterReadValue = "22222" }
            );
            _context.SaveChanges();
        }

        [Theory]
        [InlineData("2025-08-31 09:24", "54321", true)] // Invalid date format
        [InlineData("31/08/2025 12:00", "00123", false)] // Valid date format
        [InlineData("31/08/2025 09:24", "123", false)] // Padding test
        public void MeterReadingMap_CsvParsingAndPadding(string dateTimeString, string meterReadValue, bool shouldThrow)
        {
            var csvContent = $"""
                AccountId,MeterReadingDateTime,MeterReadValue
                1234,{dateTimeString},{meterReadValue}
                """;
            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, _csvConfig);
            csv.Context.RegisterClassMap<MeterReadingMap>();
            if (shouldThrow)
            {
                Assert.Throws<CsvHelper.TypeConversion.TypeConverterException>(() =>
                    csv.GetRecords<MeterReading>().ToList());
            }
            else
            {
                var records = csv.GetRecords<MeterReading>().ToList();
                Assert.Single(records);
                var record = records[0];
                Assert.Equal(1234, record.AccountId);
                if (meterReadValue.Length < 5)
                {
                    Assert.Equal(meterReadValue.PadLeft(5, '0'), record.MeterReadValue);
                }
                else
                {
                    Assert.Equal(meterReadValue, record.MeterReadValue);
                }
                if (dateTimeString == "31/08/2025 12:00")
                {
                    var expectedDate = DateTime.ParseExact(dateTimeString, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                    Assert.Equal(expectedDate, record.MeterReadingDateTime);
                }
            }
        }

        [Fact]
        public async Task ProcessMeterReadingsCsvAsync_ValidReading_ShouldReturnNoErrors()
        {
            var reading = new MeterReading
            {
                AccountId = 1234,
                MeterReadingDateTime = DateTime.UtcNow.AddDays(-1),
                MeterReadValue = "54321"
            };
            var result = await ProcessSingleReading(reading);
            Assert.Equal(1, result.SuccessfulReadings);
            Assert.Equal(0, result.FailedReadings);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ProcessMeterReadingsCsvAsync_ExistingDuplicateReading_ShouldReturnDuplicateError()
        {
            var reading = new MeterReading
            {
                AccountId = 1234,
                MeterReadingDateTime = new DateTime(2024, 1, 1, 10, 0, 0),
                MeterReadValue = "99999"
            };
            var result = await ProcessSingleReading(reading);
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(1, result.FailedReadings);
            Assert.Single(result.Errors);
            Assert.Contains("Duplicate reading for AccountId 1234", result.Errors[0]);
        }

        [Theory]
        [InlineData("100000")]
        [InlineData("")]
        public async Task ProcessMeterReadingsCsvAsync_InvalidMeterValue_ShouldReturnMeterValueError(string invalidValue)
        {
            var reading = new MeterReading
            {
                AccountId = 1234,
                MeterReadingDateTime = DateTime.UtcNow.AddDays(-1),
                MeterReadValue = invalidValue
            };
            var result = await ProcessSingleReading(reading);
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(1, result.FailedReadings);
            Assert.True(result.Errors.Any());
        }

        [Fact]
        public async Task ProcessMeterReadingsCsvAsync_AllValidationFailures_ShouldReturnAllErrors()
        {
            var futureDate = DateTime.UtcNow.AddDays(1);
            var reading = new MeterReading
            {
                AccountId = 9999,
                MeterReadingDateTime = futureDate,
                MeterReadValue = "ABC12"
            };
            var result = await ProcessSingleReading(reading);
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(1, result.FailedReadings);
            Assert.Equal(3, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.Contains("AccountId 9999 does not exist"));
            Assert.Contains(result.Errors, e => e.Contains("Invalid meter reading value 'ABC12'"));
            Assert.Contains(result.Errors, e => e.Contains("date cannot be in the future"));
        }

        [Fact]
        public async Task ProcessMeterReadingsCsvAsync_ExceptionDuringProcessing_ShouldReturnErrorResult()
        {
            _context.Dispose();
            var csvContent = """
                AccountId,MeterReadingDateTime,MeterReadValue
                1234,31/08/2025 09:24,54321
                """;
            var result = await ProcessCsvContent(csvContent);
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Contains("Error processing CSV file"));
        }


        [Fact]
        public async Task ProcessMeterReadingsCsvAsync_MissingHeaderRow_ShouldReturnError()
        {
            var csvContent = """
                1234,31/08/2025 09:24,54321
                5678,31/08/2025 10:30,67890
                """;
            var result = await ProcessCsvContent(csvContent);
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Contains("CSV file is missing header"));
        }

        [Fact]
        public async Task ProcessMeterReadingsCsvAsync_ExtraColumnsWithRequiredColumns_ShouldSucceed()
        {
            var csvContent = """
                AccountId,MeterReadingDateTime,MeterReadValue,ExtraColumn
                1234,31/08/2025 09:24,54321,SomeValue
                5678,31/08/2025 10:30,00890,AnotherValue
                """;
            var result = await ProcessCsvContent(csvContent);
            Assert.Equal(2, result.SuccessfulReadings);
            Assert.Equal(0, result.FailedReadings);
            Assert.Empty(result.Errors);
        }
    }
}
