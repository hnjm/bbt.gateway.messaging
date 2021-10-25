using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class reset2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Operators",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 1, 60, 1, 1, false });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 2, 60, 1, 2, false });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 3, 60, 1, 3, false });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 4, 60, 1, 4, false });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 5, 60, 1, 5, false });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Operators",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Operators",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Operators",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Operators",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Operators",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Operators",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);
        }
    }
}
