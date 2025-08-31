using CsvHelper;
using EnergyMeterApi.Data;
using EnergyMeterApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using EnsekTest.EnergyMeterApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        // TODO: Production: Restrict to specific origins from configuration
        
    }
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 7188;
});

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(60);
});

builder.Services.AddDbContext<EnergyMeterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRestApiServices, RestApiService>();

var app = builder.Build();

//seed data from CSV file if database is empty
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EnergyMeterDbContext>();
    
    try
    {
        await context.Database.EnsureCreatedAsync();
        
        using var reader = new StreamReader("Data/Test_Accounts.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var csvRecords = csv.GetRecords<Account>().ToList();
        
        var duplicateAccountIds = csvRecords
            .GroupBy(r => r.AccountId)
            .Where(g => g.Count() > 1)
            .Select(g => new { AccountId = g.Key, Count = g.Count() })
            .ToList();
            
        if (duplicateAccountIds.Any())
        {
            var duplicateDetails = string.Join(", ", duplicateAccountIds.Select(d => $"AccountId {d.AccountId} (appears {d.Count} times)"));
            throw new InvalidOperationException($"CSV file contains duplicate AccountIds: {duplicateDetails}");
        }

        var existingAccountIds = await context.Accounts
            .Select(a => a.AccountId)
            .ToHashSetAsync();

        var newAccounts = csvRecords
            .Where(record => !existingAccountIds.Contains(record.AccountId))
            .ToList();

        if (newAccounts.Any())
        {
            context.Accounts.AddRange(newAccounts);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding data: {ex.Message}");// would normally use a logging framework
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
