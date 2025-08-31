using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace EnsekTest.EnergyMeterApi.Helpers
{
    public class PaddedMeterReadValueConverter : ITypeConverter
    {
        public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            // CSV validation ensures text is never null/empty/whitespace, but keep defensive check
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Remove any whitespace and pad with leading zeros if less than 5 characters
            var cleanedText = text.Trim();
            return cleanedText.Length < 5 ? cleanedText.PadLeft(5, '0') : cleanedText;
        }

        public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
        {
            return value?.ToString();
        }
    }
}