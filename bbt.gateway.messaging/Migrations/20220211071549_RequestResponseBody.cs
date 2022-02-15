using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class RequestResponseBody : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("42d703ce-aa7b-4b95-8380-b16d4312cf94"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("c3f3bab4-3d29-497e-9e01-49021ebda32c"));

            migrationBuilder.AddColumn<string>(
                name: "ResponseMessage",
                table: "OtpTrackingLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestBody",
                table: "OtpResponseLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseBody",
                table: "OtpResponseLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("b4ec502a-0a79-4a19-a3b4-fdb128e46b33"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("ba5d33b7-1e3e-4d98-a6ca-438409516e63"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("b4ec502a-0a79-4a19-a3b4-fdb128e46b33"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("ba5d33b7-1e3e-4d98-a6ca-438409516e63"));

            migrationBuilder.DropColumn(
                name: "ResponseMessage",
                table: "OtpTrackingLog");

            migrationBuilder.DropColumn(
                name: "RequestBody",
                table: "OtpResponseLog");

            migrationBuilder.DropColumn(
                name: "ResponseBody",
                table: "OtpResponseLog");

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("42d703ce-aa7b-4b95-8380-b16d4312cf94"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("c3f3bab4-3d29-497e-9e01-49021ebda32c"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });
        }
    }
}
