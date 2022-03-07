using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Headers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    BusinessLine = table.Column<string>(type: "text", nullable: true),
                    Branch = table.Column<int>(type: "integer", nullable: true),
                    SmsSender = table.Column<int>(type: "integer", nullable: false),
                    SmsPrefix = table.Column<string>(type: "text", nullable: true),
                    SmsSuffix = table.Column<string>(type: "text", nullable: true),
                    EmailTemplatePrefix = table.Column<string>(type: "text", nullable: true),
                    EmailTemplateSuffix = table.Column<string>(type: "text", nullable: true),
                    SmsTemplatePrefix = table.Column<string>(type: "text", nullable: true),
                    SmsTemplateSuffix = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Headers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ControlDaysForOtp = table.Column<int>(type: "integer", nullable: false),
                    AuthanticationService = table.Column<string>(type: "text", nullable: true),
                    SendService = table.Column<string>(type: "text", nullable: true),
                    QueryService = table.Column<string>(type: "text", nullable: true),
                    AuthToken = table.Column<string>(type: "text", nullable: true),
                    TokenCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TokenExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    User = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    UseIvnWhenDeactive = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SupportDeskMail = table.Column<string>(type: "text", nullable: true),
                    SupportDeskPhone = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhoneConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Phone_CountryCode = table.Column<int>(type: "integer", nullable: true),
                    Phone_Prefix = table.Column<int>(type: "integer", nullable: true),
                    Phone_Number = table.Column<int>(type: "integer", nullable: true),
                    CustomerNo = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Operator = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlackListEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy_Name = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedBy_Name = table.Column<string>(type: "text", nullable: true),
                    ResolvedBy_ItemId = table.Column<string>(type: "text", nullable: true),
                    ResolvedBy_Action = table.Column<string>(type: "text", nullable: true),
                    ResolvedBy_Identity = table.Column<string>(type: "text", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneConfigurationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Phone_CountryCode = table.Column<int>(type: "integer", nullable: true),
                    Phone_Prefix = table.Column<int>(type: "integer", nullable: true),
                    Phone_Number = table.Column<int>(type: "integer", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy_Name = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    RelatedId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy_Name = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneConfigurationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    OperatorResponseCode = table.Column<int>(type: "integer", nullable: false),
                    OperatorResponseMessage = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Name = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlackListEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    ParameterMaster = table.Column<string>(type: "text", nullable: true),
                    ParameterSlave = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Name = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_ItemId = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Action = table.Column<string>(type: "text", nullable: true),
                    CreatedBy_Identity = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: true),
                    ResponseCode = table.Column<int>(type: "integer", nullable: false),
                    ResponseMessage = table.Column<string>(type: "text", nullable: true),
                    StatusQueryId = table.Column<string>(type: "text", nullable: true),
                    TrackingStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    ResponseBody = table.Column<string>(type: "text", nullable: true),
                    OtpRequestLogId = table.Column<Guid>(type: "uuid", nullable: true)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResponseMessage = table.Column<string>(type: "text", nullable: true),
                    Detail = table.Column<string>(type: "text", nullable: true),
                    QueriedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpTrackingLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpTrackingLog_OtpResponseLog_LogId",
                        column: x => x.LogId,
                        principalTable: "OtpResponseLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Headers",
                columns: new[] { "Id", "Branch", "BusinessLine", "ContentType", "EmailTemplatePrefix", "EmailTemplateSuffix", "SmsPrefix", "SmsSender", "SmsSuffix", "SmsTemplatePrefix", "SmsTemplateSuffix" },
                values: new object[,]
                {
                    { new Guid("96de8e85-83ad-4e57-83fa-6e0925fd3bab"), null, null, 0, "generic", null, "Dear Honey,", 1, ":)", "generic", null },
                    { new Guid("b1b6e3af-7473-4c7d-91ca-26e2489a44a4"), 2000, null, 0, "on", null, "OBEY:", 2, null, "on", null }
                });

            migrationBuilder.InsertData(
                table: "Operators",
                columns: new[] { "Id", "AuthToken", "AuthanticationService", "ControlDaysForOtp", "Password", "QueryService", "SendService", "Status", "SupportDeskMail", "SupportDeskPhone", "TokenCreatedAt", "TokenExpiredAt", "Type", "UseIvnWhenDeactive", "User" },
                values: new object[,]
                {
                    { 1, null, null, 60, null, null, null, 1, null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, null },
                    { 2, null, null, 60, null, null, null, 1, null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, false, null },
                    { 3, null, null, 60, null, null, null, 1, null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, false, null },
                    { 4, null, null, 60, null, null, null, 1, null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, false, null },
                    { 5, null, null, 60, null, null, null, 1, null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, null }
                });

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
                name: "IX_OtpTrackingLog_LogId",
                table: "OtpTrackingLog",
                column: "LogId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneConfigurationLog_PhoneId",
                table: "PhoneConfigurationLog",
                column: "PhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneConfigurations_Id",
                table: "PhoneConfigurations",
                column: "Id");

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
