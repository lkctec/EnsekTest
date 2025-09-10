using System.ComponentModel.DataAnnotations;

namespace EnergyMeterApi.Models;

/// <summary>
/// Represents a meter reading record for a specific account at a point in time.
/// </summary>
public class MeterReading
{
    /// <summary>
    /// Gets or sets the unique identifier for this meter reading record.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the account identifier the meter reading belongs to.
    /// Must correspond to an existing account in the system.
    /// This is a required field.
    /// </summary>
    [Required]
    public int AccountId { get; set; }
    public DateTime MeterReadingDateTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the meter reading was taken
    /// This is a required field.
    /// </summary>
    [Required]
    [StringLength(5, MinimumLength = 1)]
    public string MeterReadValue { get; set; } = string.Empty;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
