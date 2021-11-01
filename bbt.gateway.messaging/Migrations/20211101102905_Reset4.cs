using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class Reset4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("6c51c26f-fadd-4cf5-a65a-ee247e3d9de5"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("78105049-7b39-4e65-aa04-779916b11a15"));

            migrationBuilder.RenameColumn(
                name: "auto_id",
                table: "PhoneConfigurations",
                newName: "_$id");

            migrationBuilder.RenameIndex(
                name: "IX_PhoneConfigurations_auto_id",
                table: "PhoneConfigurations",
                newName: "IX_PhoneConfigurations__$id");

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("3629e2fe-a77e-4eb9-ae65-29f28acde8f9"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("f6dc3169-bb75-49a9-b37b-5cf217c87938"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("3629e2fe-a77e-4eb9-ae65-29f28acde8f9"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("f6dc3169-bb75-49a9-b37b-5cf217c87938"));

            migrationBuilder.RenameColumn(
                name: "_$id",
                table: "PhoneConfigurations",
                newName: "auto_id");

            migrationBuilder.RenameIndex(
                name: "IX_PhoneConfigurations__$id",
                table: "PhoneConfigurations",
                newName: "IX_PhoneConfigurations_auto_id");

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("6c51c26f-fadd-4cf5-a65a-ee247e3d9de5"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("78105049-7b39-4e65-aa04-779916b11a15"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });
        }
    }
}
