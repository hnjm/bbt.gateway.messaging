using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class reset3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsTrackingLog_SendOtpResponseLog_SendOtpResponseLogId",
                table: "SmsTrackingLog");

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("51625a13-4ce6-4408-a61a-3fb3952a014f"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("cc5df148-8d31-4b23-ac59-7c3d3fe0bbcc"));

            migrationBuilder.AlterColumn<Guid>(
                name: "SendOtpResponseLogId",
                table: "SmsTrackingLog",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OtpBlackListEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("bd991e2a-93b6-4ae7-a683-4b6944d5aa6a"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("f961463c-2014-4cc8-85cc-ace00e6c4541"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.AddForeignKey(
                name: "FK_SmsTrackingLog_SendOtpResponseLog_SendOtpResponseLogId",
                table: "SmsTrackingLog",
                column: "SendOtpResponseLogId",
                principalTable: "SendOtpResponseLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsTrackingLog_SendOtpResponseLog_SendOtpResponseLogId",
                table: "SmsTrackingLog");

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("bd991e2a-93b6-4ae7-a683-4b6944d5aa6a"));

            migrationBuilder.DeleteData(
                table: "Headers",
                keyColumn: "Id",
                keyValue: new Guid("f961463c-2014-4cc8-85cc-ace00e6c4541"));

            migrationBuilder.AlterColumn<Guid>(
                name: "SendOtpResponseLogId",
                table: "SmsTrackingLog",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OtpBlackListEntries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("51625a13-4ce6-4408-a61a-3fb3952a014f"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("cc5df148-8d31-4b23-ac59-7c3d3fe0bbcc"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });

            migrationBuilder.AddForeignKey(
                name: "FK_SmsTrackingLog_SendOtpResponseLog_SendOtpResponseLogId",
                table: "SmsTrackingLog",
                column: "SendOtpResponseLogId",
                principalTable: "SendOtpResponseLog",
                principalColumn: "Id");
        }
    }
}
