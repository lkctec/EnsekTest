using CsvHelper.Configuration;
using EnergyMeterApi.Models;

namespace EnsekTest.EnergyMeterApi.Helpers
{
    public class MeterReadingMap : ClassMap<MeterReading>
    {
        public MeterReadingMap()
        {
            Map(m => m.AccountId).Name("AccountId").Validate(field => !string.IsNullOrWhiteSpace(field.Field));
            Map(m => m.MeterReadingDateTime)
                .Name("MeterReadingDateTime")
                .TypeConverterOption.Format("dd/MM/yyyy HH:mm")
                .Validate(field => !string.IsNullOrWhiteSpace(field.Field));
            Map(m => m.MeterReadValue)
                .Name("MeterReadValue")
                .TypeConverter<PaddedMeterReadValueConverter>()
                .Validate(field => !string.IsNullOrWhiteSpace(field.Field));
        }
    }
}