using EnergyMeterApi.Data;
using EnergyMeterApi.Models;
using CsvHelper;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using CsvHelper.Configuration;
using EnsekTest.EnergyMeterApi.Helpers;

namespace EnsekTest.EnergyMeterApi.Services
{
    public class RestApiService : IRestApiServices
    {
        private readonly EnergyMeterDbContext _context;

        public RestApiService(EnergyMeterDbContext context)
        {
            _context = context;
        }

        public async Task<MeterReadingUploadResult> ProcessMeterReadingsCsvAsync(Stream csvStream)
        {
            var result = new MeterReadingUploadResult();

            try
            {
                using var reader = new StreamReader(csvStream);
                
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                };
                
                using var csv = new CsvReader(reader, config);
                
                csv.Context.RegisterClassMap<MeterReadingMap>();

                var headerValidationError = ValidateCsvHeaders(csv);
                if (headerValidationError != null)
                {
                    result.Errors.Add(headerValidationError);
                    return result;
                }
                
                var csvRecords = new List<MeterReading>();
                
                try
                {
                    csvRecords = csv.GetRecords<MeterReading>().ToList();
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"An error occurred getting meter readings from the csv: {ex.Message}");
                    result.FailedReadings++;
                    return result;
                }

                var validAccountIds = await _context.Accounts
                    .Select(a => a.AccountId)
                    .Distinct().ToListAsync();

                var existingReadings = await _context.MeterReadings
                    .Select(mr => $"{mr.AccountId}_{mr.MeterReadingDateTime:yyyy-MM-dd_HH:mm:ss}")
                    .Distinct().ToListAsync();


                var csvDuplicates = csvRecords
                    .GroupBy(r => new { r.AccountId, r.MeterReadingDateTime })
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (csvDuplicates.Any())
                {
                    foreach (var duplicate in csvDuplicates)
                    {
                        result.FailedReadings += duplicate.Count();
                        result.Errors.Add($"CSV contains duplicate readings for AccountId {duplicate.Key.AccountId} at {duplicate.Key.MeterReadingDateTime}");
                    }
                    return result;
                }

                var validReadings = new List<MeterReading>();

                foreach (var reading in csvRecords)
                {
                    var validationErrors = ValidateMeterReading(reading, validAccountIds, existingReadings);
                    
                    if (validationErrors.Any())
                    {
                        result.FailedReadings++;
                        result.Errors.AddRange(validationErrors);
                        continue;
                    }

                    validReadings.Add(reading);
                    result.SuccessfulReadings++;
                    
                    var readingKey = $"{reading.AccountId}_{reading.MeterReadingDateTime:yyyy-MM-dd_HH:mm:ss}";
                    existingReadings.Add(readingKey);
                }

                if (validReadings.Any())
                {
                    _context.MeterReadings.AddRange(validReadings);
                    await _context.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error processing CSV file: {ex.Message}");
                result.FailedReadings = result.SuccessfulReadings + result.FailedReadings;
                result.SuccessfulReadings = 0;
                return result;
            }
        }

        private string? ValidateCsvHeaders(CsvReader csv)
        {
            try
            {
                csv.Read();
                csv.ReadHeader();
                
                var requiredHeaders = new[] { "AccountId", "MeterReadingDateTime", "MeterReadValue" };
                var actualHeaders = csv.HeaderRecord;
                
                if (actualHeaders == null || actualHeaders.Length == 0)
                {
                    return "CSV file is missing header row";
                }
                
                var missingHeaders = requiredHeaders
                    .Where(required => !actualHeaders.Contains(required, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                
                if (missingHeaders.Any())
                {
                    return $"CSV file is missing header fields: {string.Join(", ", missingHeaders)}";
                }
                
                return null; // No validation errors
            }
            catch (Exception ex)
            {
                return $"Error reading CSV headers: {ex.Message}";
            }
        }

        private List<string> ValidateMeterReading(MeterReading reading, IEnumerable<int> validAccountIds, IEnumerable<string> existingReadings)
        {
            var errors = new List<string>();

            // Validate AccountId exists
            if (!validAccountIds.Contains(reading.AccountId))
            {
                errors.Add($"AccountId {reading.AccountId} does not exist");
            }

            // Check for duplicate reading (same account and datetime)
            var readingKey = $"{reading.AccountId}_{reading.MeterReadingDateTime:yyyy-MM-dd_HH:mm:ss}";
            if (existingReadings.Contains(readingKey))
            {
                errors.Add($"Duplicate reading for AccountId {reading.AccountId} at {reading.MeterReadingDateTime}");
            }

            // Validate meter reading value format - must be exactly 5 digits (NNNNN)
            if (string.IsNullOrWhiteSpace(reading.MeterReadValue))
            {
                errors.Add($"MeterReadValue cannot be empty for AccountId {reading.AccountId}");
            }
            else if (reading.MeterReadValue.Length != 5)
            {
                errors.Add($"Invalid meter reading value '{reading.MeterReadValue}' for AccountId {reading.AccountId} - value must be exactly 5 digits (NNNNN format)");
            }
            else if (!reading.MeterReadValue.All(char.IsDigit))
            {
                errors.Add($"Invalid meter reading value '{reading.MeterReadValue}' for AccountId {reading.AccountId} - value must contain only digits");
            }

            // Validate reading date is not in the future
            if (reading.MeterReadingDateTime > DateTime.UtcNow)
            {
                errors.Add($"Invalid reading date {reading.MeterReadingDateTime} for AccountId {reading.AccountId} - date cannot be in the future");
            }

            return errors;
        }
    }
}
