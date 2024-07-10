using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedJamesTKirk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AstronautDetail",
                keyColumn: "Id",
                keyValue: 1,
                column: "CareerStartDate",
                value: new DateTime(2024, 7, 10, 13, 22, 49, 843, DateTimeKind.Local).AddTicks(8807));

            migrationBuilder.UpdateData(
                table: "AstronautDuty",
                keyColumn: "Id",
                keyValue: 1,
                column: "DutyStartDate",
                value: new DateTime(2024, 7, 10, 13, 22, 49, 843, DateTimeKind.Local).AddTicks(8927));

            migrationBuilder.InsertData(
                table: "Person",
                columns: new[] { "Id", "Name" },
                values: new object[] { 14, "James" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Person",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.UpdateData(
                table: "AstronautDetail",
                keyColumn: "Id",
                keyValue: 1,
                column: "CareerStartDate",
                value: new DateTime(2024, 7, 10, 10, 55, 23, 862, DateTimeKind.Local).AddTicks(7984));

            migrationBuilder.UpdateData(
                table: "AstronautDuty",
                keyColumn: "Id",
                keyValue: 1,
                column: "DutyStartDate",
                value: new DateTime(2024, 7, 10, 10, 55, 23, 862, DateTimeKind.Local).AddTicks(8112));
        }
    }
}
