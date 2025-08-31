using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsekTest.EnergyMeterApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMeterReadValueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_MeterReadValue_NumericOnly",
                table: "MeterReadings",
                sql: "MeterReadValue NOT LIKE '%[^0-9]%'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MeterReadValue_NumericOnly",
                table: "MeterReadings");
        }
    }
}
