using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class devir : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("4fc3cd9a-87b2-47c7-afaa-62d50bcf0844"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("817ece01-c4e2-4bb0-97cf-f68195af17c4"));

            migrationBuilder.AddColumn<string>(
                name: "AuthanticationService",
                table: "Operators",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Operators",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryService",
                table: "Operators",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SendService",
                table: "Operators",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "Operators",
                type: "TEXT",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("9f91d62a-d280-4bec-92eb-6eee6fafa285"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("d83edeb9-9b3c-493c-b1c4-0487d5485eb7"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("9f91d62a-d280-4bec-92eb-6eee6fafa285"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("d83edeb9-9b3c-493c-b1c4-0487d5485eb7"));

            migrationBuilder.DropColumn(
                name: "AuthanticationService",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "QueryService",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "SendService",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "User",
                table: "Operators");

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("4fc3cd9a-87b2-47c7-afaa-62d50bcf0844"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("817ece01-c4e2-4bb0-97cf-f68195af17c4"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });
        }
    }
}
