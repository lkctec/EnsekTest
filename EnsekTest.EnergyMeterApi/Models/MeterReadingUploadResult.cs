namespace EnergyMeterApi.Models;

public class MeterReadingUploadResult
{
    public int SuccessfulReadings { get; set; }
    public int FailedReadings { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
