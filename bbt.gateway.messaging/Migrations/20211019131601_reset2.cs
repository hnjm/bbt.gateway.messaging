using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace bbt.gateway.messaging.Migrations
{
    public partial class reset2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone_prefix",
                table: "PhoneConfigurations",
                newName: "Phone_Prefix");

            migrationBuilder.RenameColumn(
                name: "Phone_prefix",
                table: "OtpBlackListEntries",
                newName: "Phone_Prefix");

            migrationBuilder.CreateTable(
                name: "OtpOperatorExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Operator = table.Column<int>(type: "INTEGER", nullable: false),
                    ReplaceWith = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Name = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpOperatorExceptions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtpOperatorExceptions");

            migrationBuilder.RenameColumn(
                name: "Phone_Prefix",
                table: "PhoneConfigurations",
                newName: "Phone_prefix");

            migrationBuilder.RenameColumn(
                name: "Phone_Prefix",
                table: "OtpBlackListEntries",
                newName: "Phone_prefix");
        }
    }
}
