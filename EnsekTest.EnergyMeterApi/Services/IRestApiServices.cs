using EnergyMeterApi.Models;
using System.IO;
using System.Threading.Tasks;

namespace EnsekTest.EnergyMeterApi.Services
{
    public interface IRestApiServices
    {
        Task<MeterReadingUploadResult> ProcessMeterReadingsCsvAsync(Stream csvStream);
    }
}
