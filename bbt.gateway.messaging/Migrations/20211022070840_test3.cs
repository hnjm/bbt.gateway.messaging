using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class test3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SendOtpRequestLog_PhoneConfigurations_PhoneId",
                table: "SendOtpRequestLog");

            migrationBuilder.RenameColumn(
                name: "PhoneId",
                table: "SendOtpRequestLog",
                newName: "PhoneConfigurationId");

            migrationBuilder.RenameIndex(
                name: "IX_SendOtpRequestLog_PhoneId",
                table: "SendOtpRequestLog",
                newName: "IX_SendOtpRequestLog_PhoneConfigurationId");

            migrationBuilder.AddColumn<int>(
                name: "Phone_CountryCode",
                table: "SendOtpRequestLog",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Phone_Number",
                table: "SendOtpRequestLog",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Phone_Prefix",
                table: "SendOtpRequestLog",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SendOtpRequestLog_PhoneConfigurations_PhoneConfigurationId",
                table: "SendOtpRequestLog",
                column: "PhoneConfigurationId",
                principalTable: "PhoneConfigurations",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SendOtpRequestLog_PhoneConfigurations_PhoneConfigurationId",
                table: "SendOtpRequestLog");

            migrationBuilder.DropColumn(
                name: "Phone_CountryCode",
                table: "SendOtpRequestLog");

            migrationBuilder.DropColumn(
                name: "Phone_Number",
                table: "SendOtpRequestLog");

            migrationBuilder.DropColumn(
                name: "Phone_Prefix",
                table: "SendOtpRequestLog");

            migrationBuilder.RenameColumn(
                name: "PhoneConfigurationId",
                table: "SendOtpRequestLog",
                newName: "PhoneId");

            migrationBuilder.RenameIndex(
                name: "IX_SendOtpRequestLog_PhoneConfigurationId",
                table: "SendOtpRequestLog",
                newName: "IX_SendOtpRequestLog_PhoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_SendOtpRequestLog_PhoneConfigurations_PhoneId",
                table: "SendOtpRequestLog",
                column: "PhoneId",
                principalTable: "PhoneConfigurations",
                principalColumn: "Id");
        }
    }
}
