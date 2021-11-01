using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class Reset : Migration
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
                name: "BlackListEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhoneConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
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
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackListEntries_PhoneConfigurations_PhoneConfigurationId",
                        column: x => x.PhoneConfigurationId,
                        principalTable: "PhoneConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtpRequestLogs",
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
                    table.PrimaryKey("PK_OtpRequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpRequestLogs_PhoneConfigurations_PhoneConfigurationId",
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
                    RelatedId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                name: "SmsLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhoneConfigurationId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_SmsLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsLogs_PhoneConfigurations_PhoneConfigurationId",
                        column: x => x.PhoneConfigurationId,
                        principalTable: "PhoneConfigurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BlackListEntryLog",
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
                    table.PrimaryKey("PK_BlackListEntryLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackListEntryLog_BlackListEntries_BlackListEntryId",
                        column: x => x.BlackListEntryId,
                        principalTable: "BlackListEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OtpResponseLog",
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
                    OtpRequestLogId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpResponseLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpResponseLog_OtpRequestLogs_OtpRequestLogId",
                        column: x => x.OtpRequestLogId,
                        principalTable: "OtpRequestLogs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OtpTrackingLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Detail = table.Column<string>(type: "TEXT", nullable: true),
                    QueriedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OtpResponseLogId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpTrackingLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpTrackingLog_OtpResponseLog_OtpResponseLogId",
                        column: x => x.OtpResponseLogId,
                        principalTable: "OtpResponseLog",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("e787a54d-740d-456d-8db7-ae6c84207ce0"), 2000, null, 0, "on", null, "OBEY:", "ZEUS", null, "on", null });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[] { new Guid("f388d07e-a8f5-4be0-8892-f6a163895a13"), null, null, 0, "generic", null, "Dear Honey,", "BATMAN", ":)", "generic", null });

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
                name: "IX_BlackListEntries_PhoneConfigurationId",
                table: "BlackListEntries",
                column: "PhoneConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListEntryLog_BlackListEntryId",
                table: "BlackListEntryLog",
                column: "BlackListEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpRequestLogs_PhoneConfigurationId",
                table: "OtpRequestLogs",
                column: "PhoneConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpResponseLog_OtpRequestLogId",
                table: "OtpResponseLog",
                column: "OtpRequestLogId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpTrackingLog_OtpResponseLogId",
                table: "OtpTrackingLog",
                column: "OtpResponseLogId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneConfigurationLog_PhoneId",
                table: "PhoneConfigurationLog",
                column: "PhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_PhoneConfigurationId",
                table: "SmsLogs",
                column: "PhoneConfigurationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackListEntryLog");

            migrationBuilder.DropTable(
                name: "Headers");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "OtpTrackingLog");

            migrationBuilder.DropTable(
                name: "PhoneConfigurationLog");

            migrationBuilder.DropTable(
                name: "SmsLogs");

            migrationBuilder.DropTable(
                name: "BlackListEntries");

            migrationBuilder.DropTable(
                name: "OtpResponseLog");

            migrationBuilder.DropTable(
                name: "OtpRequestLogs");

            migrationBuilder.DropTable(
                name: "PhoneConfigurations");
        }
    }
}
