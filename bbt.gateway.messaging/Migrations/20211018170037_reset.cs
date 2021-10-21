using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
                    Branch = table.Column<string>(type: "TEXT", nullable: true),
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
                name: "PhoneConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Phone_CountryCode = table.Column<int>(type: "INTEGER", nullable: true),
                    Phone_prefix = table.Column<int>(type: "INTEGER", nullable: true),
                    Phone_Number = table.Column<int>(type: "INTEGER", nullable: true),
                    Operator = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Phone_CountryCode = table.Column<int>(type: "INTEGER", nullable: true),
                    Phone_prefix = table.Column<int>(type: "INTEGER", nullable: true),
                    Phone_Number = table.Column<int>(type: "INTEGER", nullable: true),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SendOtpRequestLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhoneId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                        name: "FK_SendOtpRequestLog_PhoneConfigurations_PhoneId",
                        column: x => x.PhoneId,
                        principalTable: "PhoneConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SendOtpResponseLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Operator = table.Column<int>(type: "INTEGER", nullable: false),
                    OperatorResponseCode = table.Column<int>(type: "INTEGER", nullable: false),
                    OperatorResponseMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_SendOtpRequestLog_PhoneId",
                table: "SendOtpRequestLog",
                column: "PhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_SendOtpResponseLog_SendOtpRequestLogId",
                table: "SendOtpResponseLog",
                column: "SendOtpRequestLogId");

            migrationBuilder.CreateIndex(
                name: "IX_SendSmsLog_PhoneId",
                table: "SendSmsLog",
                column: "PhoneId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Headers");

            migrationBuilder.DropTable(
                name: "OtpBlackListEntryLog");

            migrationBuilder.DropTable(
                name: "PhoneConfigurationLog");

            migrationBuilder.DropTable(
                name: "SendOtpResponseLog");

            migrationBuilder.DropTable(
                name: "SendSmsLog");

            migrationBuilder.DropTable(
                name: "OtpBlackListEntries");

            migrationBuilder.DropTable(
                name: "SendOtpRequestLog");

            migrationBuilder.DropTable(
                name: "PhoneConfigurations");
        }
    }
}
