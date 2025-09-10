namespace EnergyMeterApi.Models;

/// <summary>
/// Gets or sets the unique identifier for the account.
/// This serves as the primary key and must match existing account records.
/// </summary>
public class Account
{
    /// <summary>
    /// Gets or sets the unique identifier for the account
    /// </summary>
    public int AccountId { get; set; }
    /// <summary>
    /// Gets or sets the customer's first name.
    /// This field is required and has a maximum length of 100 characters.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the customer's last name.
    /// This field is required and has a maximum length of 100 characters.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}
