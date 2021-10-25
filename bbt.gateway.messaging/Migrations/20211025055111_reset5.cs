using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class reset5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OperatorResponseMessage",
                table: "SendOtpResponseLog",
                newName: "ResponseMessage");

            migrationBuilder.RenameColumn(
                name: "OperatorResponseCode",
                table: "SendOtpResponseLog",
                newName: "ResponseCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResponseMessage",
                table: "SendOtpResponseLog",
                newName: "OperatorResponseMessage");

            migrationBuilder.RenameColumn(
                name: "ResponseCode",
                table: "SendOtpResponseLog",
                newName: "OperatorResponseCode");
        }
    }
}
