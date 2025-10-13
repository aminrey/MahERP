using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig26Comunication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ToName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHtml = table.Column<bool>(type: "bit", nullable: false),
                    CcEmails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BccEmails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecipientType = table.Column<byte>(type: "tinyint", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttachmentCount = table.Column<int>(type: "int", nullable: false),
                    AttachmentTotalSizeKB = table.Column<long>(type: "bigint", nullable: false),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailLog_Tbl_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailLog_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailLog_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmailQueue_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ToName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHtml = table.Column<bool>(type: "bit", nullable: false),
                    CcEmails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BccEmails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Attachments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAttemptDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueue_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailQueue_Tbl_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplate_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyPlainText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplate_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailTemplate_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailTemplate_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Settings_Tbl",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmtpHost = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SmtpPort = table.Column<int>(type: "int", nullable: false),
                    SmtpEnableSsl = table.Column<bool>(type: "bit", nullable: false),
                    SmtpUsername = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SmtpPassword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SmtpFromEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SmtpFromName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MaxAttachmentSizeMB = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings_Tbl", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "SmsProvider_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SenderNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApiUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalSettings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RemainingCredit = table.Column<long>(type: "bigint", nullable: true),
                    LastCreditCheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsProvider_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsProvider_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SmsProvider_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmsTemplate_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MessageTemplate = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TemplateType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsTemplate_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsTemplate_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SmsTemplate_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplateRecipient_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    RecipientType = table.Column<byte>(type: "tinyint", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateRecipient_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailTemplateRecipient_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailTemplateRecipient_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailTemplateRecipient_Tbl_EmailTemplate_Tbl_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "EmailTemplate_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_EmailTemplateRecipient_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmsLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RecipientType = table.Column<byte>(type: "tinyint", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ProviderMessageId = table.Column<long>(type: "bigint", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeliveryStatus = table.Column<int>(type: "int", nullable: false),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStatusCheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsLog_Tbl_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SmsLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsLog_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsLog_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsLog_Tbl_SmsProvider_Tbl_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "SmsProvider_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_SmsLog_Tbl_SmsTemplate_Tbl_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "SmsTemplate_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmsTemplateRecipient_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    RecipientType = table.Column<byte>(type: "tinyint", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsTemplateRecipient_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsTemplateRecipient_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SmsTemplateRecipient_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsTemplateRecipient_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsTemplateRecipient_Tbl_SmsTemplate_Tbl_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "SmsTemplate_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "SmsQueue_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RecipientType = table.Column<byte>(type: "tinyint", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAttemptDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SmsLogId = table.Column<int>(type: "int", nullable: true),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsQueue_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsQueue_Tbl_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SmsQueue_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsQueue_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsQueue_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsQueue_Tbl_SmsLog_Tbl_SmsLogId",
                        column: x => x.SmsLogId,
                        principalTable: "SmsLog_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsQueue_Tbl_SmsProvider_Tbl_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "SmsProvider_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_Tbl_ContactId",
                table: "EmailLog_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_Tbl_OrganizationId",
                table: "EmailLog_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_Tbl_SenderUserId",
                table: "EmailLog_Tbl",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_Tbl_UserId",
                table: "EmailLog_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueue_Tbl_RequestedByUserId",
                table: "EmailQueue_Tbl",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplate_Tbl_CreatorUserId",
                table: "EmailTemplate_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplate_Tbl_LastUpdaterUserId",
                table: "EmailTemplate_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateRecipient_Tbl_AddedByUserId",
                table: "EmailTemplateRecipient_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateRecipient_Tbl_ContactId",
                table: "EmailTemplateRecipient_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateRecipient_Tbl_OrganizationId",
                table: "EmailTemplateRecipient_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateRecipient_Tbl_TemplateId",
                table: "EmailTemplateRecipient_Tbl",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLog_Tbl_ContactId",
                table: "SmsLog_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLog_Tbl_OrganizationId",
                table: "SmsLog_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLog_Tbl_ProviderId",
                table: "SmsLog_Tbl",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLog_Tbl_SenderUserId",
                table: "SmsLog_Tbl",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLog_Tbl_TemplateId",
                table: "SmsLog_Tbl",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLog_Tbl_UserId",
                table: "SmsLog_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsProvider_Code",
                table: "SmsProvider_Tbl",
                column: "ProviderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsProvider_Tbl_CreatorUserId",
                table: "SmsProvider_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsProvider_Tbl_LastUpdaterUserId",
                table: "SmsProvider_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_Tbl_ContactId",
                table: "SmsQueue_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_Tbl_OrganizationId",
                table: "SmsQueue_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_Tbl_ProviderId",
                table: "SmsQueue_Tbl",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_Tbl_RequestedByUserId",
                table: "SmsQueue_Tbl",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_Tbl_SmsLogId",
                table: "SmsQueue_Tbl",
                column: "SmsLogId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsQueue_Tbl_UserId",
                table: "SmsQueue_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplate_Tbl_CreatorUserId",
                table: "SmsTemplate_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplate_Tbl_LastUpdaterUserId",
                table: "SmsTemplate_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplateRecipient_Tbl_AddedByUserId",
                table: "SmsTemplateRecipient_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplateRecipient_Tbl_ContactId",
                table: "SmsTemplateRecipient_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplateRecipient_Tbl_OrganizationId",
                table: "SmsTemplateRecipient_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplateRecipient_Tbl_TemplateId",
                table: "SmsTemplateRecipient_Tbl",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailLog_Tbl");

            migrationBuilder.DropTable(
                name: "EmailQueue_Tbl");

            migrationBuilder.DropTable(
                name: "EmailTemplateRecipient_Tbl");

            migrationBuilder.DropTable(
                name: "Settings_Tbl");

            migrationBuilder.DropTable(
                name: "SmsQueue_Tbl");

            migrationBuilder.DropTable(
                name: "SmsTemplateRecipient_Tbl");

            migrationBuilder.DropTable(
                name: "EmailTemplate_Tbl");

            migrationBuilder.DropTable(
                name: "SmsLog_Tbl");

            migrationBuilder.DropTable(
                name: "SmsProvider_Tbl");

            migrationBuilder.DropTable(
                name: "SmsTemplate_Tbl");
        }
    }
}
