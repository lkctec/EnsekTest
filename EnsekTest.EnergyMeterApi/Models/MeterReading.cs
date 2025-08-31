using System.ComponentModel.DataAnnotations;

namespace EnergyMeterApi.Models;

public class MeterReading
{
    public int Id { get; set; }
    [Required]
    public int AccountId { get; set; }
    public DateTime MeterReadingDateTime { get; set; }

    [Required]
    [StringLength(5, MinimumLength = 1)]
    public string MeterReadValue { get; set; } = string.Empty;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
