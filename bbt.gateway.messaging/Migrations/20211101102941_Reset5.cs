using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class Reset5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                newName: "$id");

            migrationBuilder.RenameIndex(
                name: "IX_PhoneConfigurations__$id",
                table: "PhoneConfigurations",
                newName: "IX_PhoneConfigurations_$id");

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("7fd4c8cd-1dbf-4bb9-9cbf-4fadb0e4918a"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("d2991d8f-69e6-4dba-bb39-a5c40cdb3a25"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("7fd4c8cd-1dbf-4bb9-9cbf-4fadb0e4918a"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("d2991d8f-69e6-4dba-bb39-a5c40cdb3a25"));

            migrationBuilder.RenameColumn(
                name: "$id",
                table: "PhoneConfigurations",
                newName: "_$id");

            migrationBuilder.RenameIndex(
                name: "IX_PhoneConfigurations_$id",
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
    }
}
