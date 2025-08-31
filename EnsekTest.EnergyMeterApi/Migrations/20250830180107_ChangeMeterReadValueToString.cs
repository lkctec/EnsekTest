using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsekTest.EnergyMeterApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMeterReadValueToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MeterReadValue",
                table: "MeterReadings",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MeterReadValue",
                table: "MeterReadings",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);
        }
    }
}
