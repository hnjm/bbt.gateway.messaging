using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class reset4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone_CountryCode",
                table: "OtpBlackListEntries");

            migrationBuilder.DropColumn(
                name: "Phone_Number",
                table: "OtpBlackListEntries");

            migrationBuilder.DropColumn(
                name: "Phone_Prefix",
                table: "OtpBlackListEntries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Phone_CountryCode",
                table: "OtpBlackListEntries",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Phone_Number",
                table: "OtpBlackListEntries",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Phone_Prefix",
                table: "OtpBlackListEntries",
                type: "INTEGER",
                nullable: true);
        }
    }
}
