using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class Reset2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("e787a54d-740d-456d-8db7-ae6c84207ce0"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("f388d07e-a8f5-4be0-8892-f6a163895a13"));

            migrationBuilder.AddColumn<long>(
                name: "auto_id",
                table: "PhoneConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("6c51c26f-fadd-4cf5-a65a-ee247e3d9de5"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("78105049-7b39-4e65-aa04-779916b11a15"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });

            migrationBuilder.CreateIndex(
                name: "IX_PhoneConfigurations_auto_id",
                table: "PhoneConfigurations",
                column: "auto_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhoneConfigurations_Id",
                table: "PhoneConfigurations",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhoneConfigurations_auto_id",
                table: "PhoneConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_PhoneConfigurations_Id",
                table: "PhoneConfigurations");

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("6c51c26f-fadd-4cf5-a65a-ee247e3d9de5"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("78105049-7b39-4e65-aa04-779916b11a15"));

            migrationBuilder.DropColumn(
                name: "auto_id",
                table: "PhoneConfigurations");

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("e787a54d-740d-456d-8db7-ae6c84207ce0"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("f388d07e-a8f5-4be0-8892-f6a163895a13"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });
        }
    }
}
