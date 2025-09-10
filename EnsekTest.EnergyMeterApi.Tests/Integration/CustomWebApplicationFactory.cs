using EnergyMeterApi.Data;
using EnergyMeterApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.DependencyInjection; 
using CsvHelper;
using System.Globalization;

namespace EnsekTest.EnergyMeterApi.Tests.Integration
{
    public class CustomWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(IDbContextOptionsConfiguration<EnergyMeterDbContext>));

                services.Remove(dbContextDescriptor);

                var dbConnectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbConnection));

                services.Remove(dbConnectionDescriptor);

                services.AddSingleton<DbConnection>(container =>
                {
                    var connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();
                    return connection;
                });

                services.AddDbContext<EnergyMeterDbContext>((container, options) =>
                {
                    var connection = container.GetRequiredService<DbConnection>();
                    options.UseSqlite(connection);
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<EnergyMeterDbContext>();
                context.Database.EnsureCreated();

                using var reader = new StreamReader("Data/Test_Accounts.csv");
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var csvRecords = csv.GetRecords<Account>().ToList();
                context.Accounts.AddRange(csvRecords);
                context.SaveChanges();
            });

            builder.UseEnvironment("Development");
        }
    }
}
