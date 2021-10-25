using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class reset2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("a216b033-c23e-4bbd-9db4-e34e4fc3f274"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("d97bcdfa-3469-4dd3-85a6-b8ff9021af8f"));

            migrationBuilder.DropColumn(
                name: "ParameterMaster",
                table: "PhoneConfigurationLog");

            migrationBuilder.DropColumn(
                name: "ParameterSlave",
                table: "PhoneConfigurationLog");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedId",
                table: "PhoneConfigurationLog",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("51625a13-4ce6-4408-a61a-3fb3952a014f"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("cc5df148-8d31-4b23-ac59-7c3d3fe0bbcc"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("51625a13-4ce6-4408-a61a-3fb3952a014f"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("cc5df148-8d31-4b23-ac59-7c3d3fe0bbcc"));

            migrationBuilder.DropColumn(
                name: "RelatedId",
                table: "PhoneConfigurationLog");

            migrationBuilder.AddColumn<string>(
                name: "ParameterMaster",
                table: "PhoneConfigurationLog",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParameterSlave",
                table: "PhoneConfigurationLog",
                type: "TEXT",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("a216b033-c23e-4bbd-9db4-e34e4fc3f274"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("d97bcdfa-3469-4dd3-85a6-b8ff9021af8f"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });
        }
    }
}
