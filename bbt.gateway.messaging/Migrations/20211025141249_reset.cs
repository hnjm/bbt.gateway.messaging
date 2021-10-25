using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class reset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Headers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContentType = table.Column<int>(type: "INTEGER", nullable: false),
                    BusinessLine = table.Column<string>(type: "TEXT", nullable: true),
                    Branch = table.Column<int>(type: "INTEGER", nullable: true),
                    SmsSender = table.Column<string>(type: "TEXT", nullable: true),
                    SmsPrefix = table.Column<string>(type: "TEXT", nullable: true),
                    SmsSuffix = table.Column<string>(type: "TEXT", nullable: true),
                    EmailTemplatePrefix = table.Column<string>(type: "TEXT", nullable: true),
                    EmailTemplateSuffix = table.Column<string>(type: "TEXT", nullable: true),
                    SmsTemplatePrefix = table.Column<string>(type: "TEXT", nullable: true),
                    SmsTemplateSuffix = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Headers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    ControlDaysForOtp = table.Column<int>(type: "INTEGER", nullable: false),
                    UseIvnWhenDeactive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhoneConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Phone_CountryCode = table.Column<int>(type: "INTEGER", nullable: true),
                    Phone_Prefix = table.Column<int>(type: "INTEGER", nullable: true),
                    Phone_Number = table.Column<int>(type: "INTEGER", nullable: true),
                    CustomerNo = table.Column<int>(type: "INTEGER", nullable: true),
                    Operator = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpBlackListEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhoneConfigurationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy_Name = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedBy_Name = table.Column<string>(type: "TEXT", nullable: true),
                    ResolvedBy_ItemId = table.Column<string>(type: "TEXT", nullable: true),
                    ResolvedBy_Action = table.Column<string>(type: "TEXT", nullable: true),
                    ResolvedBy_Identity = table.Column<string>(type: "TEXT", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpBlackListEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpBlackListEntries_PhoneConfigurations_PhoneConfigurationId",
                        column: x => x.PhoneConfigurationId,
                        principalTable: "PhoneConfigurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PhoneConfigurationLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhoneId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: true),
                    ParameterMaster = table.Column<string>(type: "TEXT", nullable: true),
                    ParameterSlave = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Name = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneConfigurationLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhoneConfigurationLog_PhoneConfigurations_PhoneId",
                        column: x => x.PhoneId,
                        principalTable: "PhoneConfigurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SendOtpRequestLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhoneConfigurationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Phone_CountryCode = table.Column<int>(type: "INTEGER", nullable: true),
                    Phone_Prefix = table.Column<int>(type: "INTEGER", nullable: true),
                    Phone_Number = table.Column<int>(type: "INTEGER", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy_Name = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendOtpRequestLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SendOtpRequestLog_PhoneConfigurations_PhoneConfigurationId",
                        column: x => x.PhoneConfigurationId,
                        principalTable: "PhoneConfigurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SendSmsLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhoneId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Operator = table.Column<int>(type: "INTEGER", nullable: false),
                    OperatorResponseCode = table.Column<int>(type: "INTEGER", nullable: false),
                    OperatorResponseMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Name = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendSmsLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SendSmsLog_PhoneConfigurations_PhoneId",
                        column: x => x.PhoneId,
                        principalTable: "PhoneConfigurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OtpBlackListEntryLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BlackListEntryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: true),
                    ParameterMaster = table.Column<string>(type: "TEXT", nullable: true),
                    ParameterSlave = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Name = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpBlackListEntryLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpBlackListEntryLog_OtpBlackListEntries_BlackListEntryId",
                        column: x => x.BlackListEntryId,
                        principalTable: "OtpBlackListEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SendOtpResponseLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Operator = table.Column<int>(type: "INTEGER", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: true),
                    ResponseCode = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseMessage = table.Column<string>(type: "TEXT", nullable: true),
                    StatusQueryId = table.Column<string>(type: "TEXT", nullable: true),
                    TrackingStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SendOtpRequestLogId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendOtpResponseLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SendOtpResponseLog_SendOtpRequestLog_SendOtpRequestLogId",
                        column: x => x.SendOtpRequestLogId,
                        principalTable: "SendOtpRequestLog",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmsTrackingLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Detail = table.Column<string>(type: "TEXT", nullable: true),
                    QueriedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SendOtpResponseLogId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsTrackingLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsTrackingLog_SendOtpResponseLog_SendOtpResponseLogId",
                        column: x => x.SendOtpResponseLogId,
                        principalTable: "SendOtpResponseLog",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("a216b033-c23e-4bbd-9db4-e34e4fc3f274"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("d97bcdfa-3469-4dd3-85a6-b8ff9021af8f"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 1, 60, 1, 1, false });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 2, 60, 1, 2, false });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 3, 60, 1, 3, false });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 4, 60, 1, 4, false });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "ControlDaysForOtp", "Status", "Type", "UseIvnWhenDeactive" },
                values: new object[] { 5, 60, 1, 5, false });

            migrationBuilder.CreateIndex(
                name: "IX_OtpBlackListEntries_PhoneConfigurationId",
                table: "OtpBlackListEntries",
                column: "PhoneConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpBlackListEntryLog_BlackListEntryId",
                table: "OtpBlackListEntryLog",
                column: "BlackListEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneConfigurationLog_PhoneId",
                table: "PhoneConfigurationLog",
                column: "PhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_SendOtpRequestLog_PhoneConfigurationId",
                table: "SendOtpRequestLog",
                column: "PhoneConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SendOtpResponseLog_SendOtpRequestLogId",
                table: "SendOtpResponseLog",
                column: "SendOtpRequestLogId");

            migrationBuilder.CreateIndex(
                name: "IX_SendSmsLog_PhoneId",
                table: "SendSmsLog",
                column: "PhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTrackingLog_SendOtpResponseLogId",
                table: "SmsTrackingLog",
                column: "SendOtpResponseLogId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Headers");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "OtpBlackListEntryLog");

            migrationBuilder.DropTable(
                name: "PhoneConfigurationLog");

            migrationBuilder.DropTable(
                name: "SendSmsLog");

            migrationBuilder.DropTable(
                name: "SmsTrackingLog");

            migrationBuilder.DropTable(
                name: "OtpBlackListEntries");

            migrationBuilder.DropTable(
                name: "SendOtpResponseLog");

            migrationBuilder.DropTable(
                name: "SendOtpRequestLog");

            migrationBuilder.DropTable(
                name: "PhoneConfigurations");
        }
    }
}
