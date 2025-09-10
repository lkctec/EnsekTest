namespace EnergyMeterApi.Models;

/// <summary>
/// Represents the result of a meter reading CSV upload operation.
/// Contains counts of successful and failed readings along with any error messages.
/// </summary>
public class MeterReadingUploadResult
{
    /// <summary>
    /// Gets or sets the number of meter readings that were successfully processed and saved.
    /// </summary>
    public int SuccessfulReadings { get; set; }

    /// <summary>
    /// Gets or sets the number of meter readings that failed validation or processing.
    /// </summary>
    public int FailedReadings { get; set; }

    /// <summary>
    /// Gets or sets the list of error messages describing why specific readings failed
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();
}
