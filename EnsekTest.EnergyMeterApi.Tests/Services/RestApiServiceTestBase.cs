using EnergyMeterApi.Data;
using EnergyMeterApi.Models;
using EnsekTest.EnergyMeterApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EnsekTest.EnergyMeterApi.Tests.Services
{
    public abstract class RestApiServiceTestBase : IDisposable
    {
        protected readonly EnergyMeterDbContext _context;
        protected readonly RestApiService _service;

        protected RestApiServiceTestBase()
        {
            var options = new DbContextOptionsBuilder<EnergyMeterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EnergyMeterDbContext(options);
            _service = new RestApiService(_context);

            SeedTestData();
        }

        protected virtual void SeedTestData()
        {
            // Common test accounts
            _context.Accounts.AddRange(
                new Account { AccountId = 1234, FirstName = "John", LastName = "Doe" },
                new Account { AccountId = 5678, FirstName = "Jane", LastName = "Smith" },
                new Account { AccountId = 9012, FirstName = "Bob", LastName = "Wilson" }
            );

            _context.SaveChanges();
        }

        protected static MemoryStream CreateCsvStream(string csvContent)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        }

        protected async Task<MeterReadingUploadResult> ProcessSingleReading(MeterReading reading)
        {
            var csvContent = $"""
                AccountId,MeterReadingDateTime,MeterReadValue
                {reading.AccountId},{reading.MeterReadingDateTime:dd/MM/yyyy HH:mm},{reading.MeterReadValue}
                """;
            var csvStream = CreateCsvStream(csvContent);
            return await _service.ProcessMeterReadingsCsvAsync(csvStream);
        }

        protected async Task<MeterReadingUploadResult> ProcessCsvContent(string csvContent)
        {
            var csvStream = CreateCsvStream(csvContent);
            return await _service.ProcessMeterReadingsCsvAsync(csvStream);
        }

        public virtual void Dispose()
        {
            _context.Dispose();
        }
    }
}