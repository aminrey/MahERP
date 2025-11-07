using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TellPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalTellPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MelliCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectManagerUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: false),
                    OrganizationalLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    ParentUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChatidUserTelegram = table.Column<long>(type: "bigint", nullable: true),
                    TelegramRobatId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoveUser = table.Column<bool>(type: "bit", nullable: false),
                    IsCompletelyDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletelyDeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BirthDay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_AspNetUsers_DirectManagerUserId",
                        column: x => x.DirectManagerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Branch_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ManagerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsMainBranch = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TelegramBotToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelegramBotTokenName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branch_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Branch_Tbl_Branch_Tbl_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PredefinedCopyDescription_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredefinedCopyDescription_Tbl", x => x.Id);
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
                    MaxAttachmentSizeMB = table.Column<int>(type: "int", nullable: false),
                    IsTaskingModuleEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsCrmModuleEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings_Tbl", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TaskCategory_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TaskCategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategory_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCategory_Tbl_TaskCategory_Tbl_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskCategory_Tbl_TaskCategory_Tbl_TaskCategoryId",
                        column: x => x.TaskCategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contact_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PrimaryEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SecondaryEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PrimaryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SecondaryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SecondaryPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contact_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contact_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ContactGroup_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsSystemGroup = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactGroup_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactGroup_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactGroup_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CoreNotificationSetting_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SystemId = table.Column<byte>(type: "tinyint", nullable: false),
                    NotificationTypeGeneral = table.Column<byte>(type: "tinyint", nullable: false),
                    IsSystemEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsEmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsSmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsTelegramEnabled = table.Column<bool>(type: "bit", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    SendOnHolidays = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreNotificationSetting_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoreNotificationSetting_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "Organization_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    EconomicCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LegalRepresentative = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SecondaryPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OrganizationType = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organization_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organization_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrganizationGroup_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsSystemGroup = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationGroup_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationGroup_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationGroup_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Permission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameFa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemPermission = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdaterId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permission_Tbl_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Permission_Tbl_AspNetUsers_LastUpdaterId",
                        column: x => x.LastUpdaterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Permission_Tbl_Permission_Tbl_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Permission_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PermissionLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionType = table.Column<byte>(type: "tinyint", nullable: false),
                    AccessGranted = table.Column<bool>(type: "bit", nullable: false),
                    DenialReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Role_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameFa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdaterId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_Tbl_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Role_Tbl_AspNetUsers_LastUpdaterId",
                        column: x => x.LastUpdaterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolePattern_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatternName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccessLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemPattern = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePattern_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePattern_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePattern_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "UserModulePermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModuleType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModulePermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModulePermission_Tbl_AspNetUsers_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserModulePermission_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "UserModulePreference_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUsedModule = table.Column<byte>(type: "tinyint", nullable: false),
                    LastAccessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DefaultModule = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModulePreference_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModulePreference_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchContactGroup_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchContactGroup_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchContactGroup_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchContactGroup_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BranchContactGroup_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchModulePermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchModulePermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchModulePermission_Tbl_AspNetUsers_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchModulePermission_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchOrganizationGroup_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchOrganizationGroup_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroup_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroup_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroup_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchUser_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BranchId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchUser_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchUser_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchUser_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchUser_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchUser_Tbl_Branch_Tbl_BranchId1",
                        column: x => x.BranchId1,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CoreNotification_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemId = table.Column<byte>(type: "tinyint", nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    NotificationTypeGeneral = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsClicked = table.Column<bool>(type: "bit", nullable: false),
                    ClickDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RelatedRecordId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedRecordType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedRecordTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreNotification_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoreNotification_Tbl_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoreNotification_Tbl_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CoreNotification_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Stakeholder_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonType = table.Column<byte>(type: "tinyint", nullable: false),
                    StakeholderType = table.Column<byte>(type: "tinyint", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyBrand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    EconomicCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegisteredAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LegalRepresentative = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stakeholder_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stakeholder_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stakeholder_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stakeholder_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Team_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentTeamId = table.Column<int>(type: "int", nullable: true),
                    ManagerUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    AccessLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Team_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Team_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Team_Tbl_AspNetUsers_ManagerUserId",
                        column: x => x.ManagerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Team_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Team_Tbl_Team_Tbl_ParentTeamId",
                        column: x => x.ParentTeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Team_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserActivityLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActivityType = table.Column<byte>(type: "tinyint", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivityDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessingTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    ResponseSize = table.Column<long>(type: "bigint", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    IsSensitive = table.Column<bool>(type: "bit", nullable: false),
                    ImportanceLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    DeviceType = table.Column<byte>(type: "tinyint", nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivityLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserActivityLog_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplate_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    TaskType = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddBranchManagerAsSupervisor = table.Column<bool>(type: "bit", nullable: false),
                    DefaultDurationDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplate_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTemplate_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskTemplate_Tbl_TaskCategory_Tbl_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchContact_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchContact_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchContact_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchContact_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchContact_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ContactPhone_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    PhoneType = table.Column<byte>(type: "tinyint", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactPhone_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactPhone_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactPhone_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ContactGroupMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactGroupMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactGroupMember_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactGroupMember_Tbl_ContactGroup_Tbl_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ContactGroup_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ContactGroupMember_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "BranchOrganization_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint", nullable: false),
                    IncludeAllMembers = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchOrganization_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchOrganization_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchOrganization_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchOrganization_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

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
                name: "OrganizationContact_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsDecisionMaker = table.Column<bool>(type: "bit", nullable: false),
                    ImportanceLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationContact_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationContact_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationContact_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_OrganizationContact_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationDepartment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    ParentDepartmentId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ManagerContactId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationDepartment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_Contact_Tbl_ManagerContactId",
                        column: x => x.ManagerContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_OrganizationDepartment_Tbl_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "OrganizationDepartment_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationDepartment_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationGroupMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationGroupMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationGroupMember_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationGroupMember_Tbl_OrganizationGroup_Tbl_GroupId",
                        column: x => x.GroupId,
                        principalTable: "OrganizationGroup_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_OrganizationGroupMember_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "PermissionChangeLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<byte>(type: "tinyint", nullable: false),
                    ChangeDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldSourceRoleId = table.Column<int>(type: "int", nullable: true),
                    NewSourceRoleId = table.Column<int>(type: "int", nullable: true),
                    OldIsActive = table.Column<bool>(type: "bit", nullable: false),
                    NewIsActive = table.Column<bool>(type: "bit", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionChangeLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionChangeLog_Tbl_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionChangeLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PermissionChangeLog_Tbl_Permission_Tbl_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermission_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolePermission_Tbl_Permission_Tbl_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Tbl_Role_Tbl_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceRoleId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsManuallyModified = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_AspNetUsers_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_Permission_Tbl_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermission_Tbl_Role_Tbl_SourceRoleId",
                        column: x => x.SourceRoleId,
                        principalTable: "Role_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRole_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRole_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_Tbl_Role_Tbl_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePatternDetails_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolePatternId = table.Column<int>(type: "int", nullable: false),
                    ControllerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CanRead = table.Column<bool>(type: "bit", nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CanApprove = table.Column<bool>(type: "bit", nullable: false),
                    DataAccessLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePatternDetails_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePatternDetails_Tbl_RolePattern_Tbl_RolePatternId",
                        column: x => x.RolePatternId,
                        principalTable: "RolePattern_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRolePattern_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RolePatternId = table.Column<int>(type: "int", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRolePattern_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRolePattern_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRolePattern_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRolePattern_Tbl_RolePattern_Tbl_RolePatternId",
                        column: x => x.RolePatternId,
                        principalTable: "RolePattern_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "CoreNotificationDelivery_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoreNotificationId = table.Column<int>(type: "int", nullable: false),
                    DeliveryMethod = table.Column<byte>(type: "tinyint", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    AttemptDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NextRetryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreNotificationDelivery_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoreNotificationDelivery_Tbl_CoreNotification_Tbl_CoreNotificationId",
                        column: x => x.CoreNotificationId,
                        principalTable: "CoreNotification_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoreNotificationDetail_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoreNotificationId = table.Column<int>(type: "int", nullable: false),
                    NotificationTypeSpecific = table.Column<byte>(type: "tinyint", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreNotificationDetail_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoreNotificationDetail_Tbl_CoreNotification_Tbl_CoreNotificationId",
                        column: x => x.CoreNotificationId,
                        principalTable: "CoreNotification_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchTaskCategoryStakeholder_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    TaskCategoryId = table.Column<int>(type: "int", nullable: false),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TaskCategoryId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchTaskCategoryStakeholder_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchTaskCategoryStakeholder_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchTaskCategoryStakeholder_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchTaskCategoryStakeholder_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchTaskCategoryStakeholder_Tbl_TaskCategory_Tbl_TaskCategoryId",
                        column: x => x.TaskCategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchTaskCategoryStakeholder_Tbl_TaskCategory_Tbl_TaskCategoryId1",
                        column: x => x.TaskCategoryId1,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contract_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ContractType = table.Column<byte>(type: "tinyint", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Terms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contract_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contract_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contract_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contract_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderBranch_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StakeholderId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderBranch_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderBranch_Tbl_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StakeholderBranch_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderBranch_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderBranch_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderBranch_Tbl_Stakeholder_Tbl_StakeholderId1",
                        column: x => x.StakeholderId1,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StakeholderContact_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    ImportanceLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsDecisionMaker = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderContact_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderContact_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderContact_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderCRM_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    LeadSource = table.Column<byte>(type: "tinyint", nullable: false),
                    SalesStage = table.Column<byte>(type: "tinyint", nullable: false),
                    LastContactDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PotentialValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreditRating = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Preferences = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmployeeCount = table.Column<int>(type: "int", nullable: true),
                    AnnualRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalesRepUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderCRM_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderCRM_Tbl_AspNetUsers_SalesRepUserId",
                        column: x => x.SalesRepUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderCRM_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskViewPermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GranteeUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionType = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TargetTeamId = table.Column<int>(type: "int", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskViewPermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_AspNetUsers_GranteeUserId",
                        column: x => x.GranteeUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_Team_Tbl_TargetTeamId",
                        column: x => x.TargetTeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskViewPermission_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TeamModulePermission_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamModulePermission_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamModulePermission_Tbl_AspNetUsers_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamModulePermission_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamPosition_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PowerLevel = table.Column<int>(type: "int", nullable: false),
                    CanViewSubordinateTasks = table.Column<bool>(type: "bit", nullable: false),
                    CanViewPeerTasks = table.Column<bool>(type: "bit", nullable: false),
                    MaxMembers = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamPosition_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamPosition_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamPosition_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamPosition_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskSchedule_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaskTemplateId = table.Column<int>(type: "int", nullable: false),
                    RecurrenceType = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecurrenceInterval = table.Column<int>(type: "int", nullable: false),
                    WeekDays = table.Column<byte>(type: "tinyint", nullable: false),
                    MonthDay = table.Column<byte>(type: "tinyint", nullable: false),
                    YearMonth = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastRunTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRunTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRunStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    LastRunErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TaskTemplateId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSchedule_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSchedule_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskSchedule_Tbl_AspNetUsers_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskSchedule_Tbl_TaskTemplate_Tbl_TaskTemplateId",
                        column: x => x.TaskTemplateId,
                        principalTable: "TaskTemplate_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskSchedule_Tbl_TaskTemplate_Tbl_TaskTemplateId1",
                        column: x => x.TaskTemplateId1,
                        principalTable: "TaskTemplate_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplateOperation_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationOrder = table.Column<int>(type: "int", nullable: false),
                    TaskTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplateOperation_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTemplateOperation_Tbl_TaskTemplate_Tbl_TaskTemplateId",
                        column: x => x.TaskTemplateId,
                        principalTable: "TaskTemplate_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskTemplateOperation_Tbl_TaskTemplate_Tbl_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "TaskTemplate_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchContactGroupMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchGroupId = table.Column<int>(type: "int", nullable: false),
                    BranchContactId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchContactGroupMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchContactGroupMember_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchContactGroupMember_Tbl_BranchContactGroup_Tbl_BranchGroupId",
                        column: x => x.BranchGroupId,
                        principalTable: "BranchContactGroup_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BranchContactGroupMember_Tbl_BranchContact_Tbl_BranchContactId",
                        column: x => x.BranchContactId,
                        principalTable: "BranchContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "BranchOrganizationGroupMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchGroupId = table.Column<int>(type: "int", nullable: false),
                    BranchOrganizationId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchOrganizationGroupMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroupMember_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroupMember_Tbl_BranchOrganizationGroup_Tbl_BranchGroupId",
                        column: x => x.BranchGroupId,
                        principalTable: "BranchOrganizationGroup_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BranchOrganizationGroupMember_Tbl_BranchOrganization_Tbl_BranchOrganizationId",
                        column: x => x.BranchOrganizationId,
                        principalTable: "BranchOrganization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentPosition_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PowerLevel = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    MinSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CanHireSubordinates = table.Column<bool>(type: "bit", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentPosition_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentPosition_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentPosition_Tbl_OrganizationDepartment_Tbl_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "OrganizationDepartment_Tbl",
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

            migrationBuilder.CreateTable(
                name: "ActivityBase_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivityType = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    StakeholderId = table.Column<int>(type: "int", nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityBase_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityBase_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityBase_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityBase_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityBase_Tbl_Contract_Tbl_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contract_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityBase_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMInteraction_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CRMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CRMType = table.Column<byte>(type: "tinyint", nullable: false),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    Result = table.Column<byte>(type: "tinyint", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MeetingLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StakeholderId = table.Column<int>(type: "int", nullable: true),
                    StakeholderContactId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextFollowUpNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMInteraction_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMInteraction_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMInteraction_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMInteraction_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMInteraction_Tbl_Contract_Tbl_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contract_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMInteraction_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                        column: x => x.StakeholderContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMInteraction_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderOrganization_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentOrganizationId = table.Column<int>(type: "int", nullable: true),
                    ManagerContactId = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderOrganization_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_StakeholderContact_Tbl_ManagerContactId",
                        column: x => x.ManagerContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_StakeholderOrganization_Tbl_ParentOrganizationId",
                        column: x => x.ParentOrganizationId,
                        principalTable: "StakeholderOrganization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganization_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    RoleDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MembershipType = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMember_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamMember_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamMember_Tbl_TeamPosition_Tbl_PositionId",
                        column: x => x.PositionId,
                        principalTable: "TeamPosition_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamMember_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tasks_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskType = table.Column<byte>(type: "tinyint", nullable: false),
                    TaskTypeInput = table.Column<byte>(type: "tinyint", nullable: false),
                    VisibilityLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    DisplayLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    Important = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupervisorApproved = table.Column<bool>(type: "bit", nullable: true),
                    SupervisorApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerApproved = table.Column<bool>(type: "bit", nullable: true),
                    ManagerApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    CreationMode = table.Column<byte>(type: "tinyint", nullable: false),
                    IsIndependentCompletion = table.Column<bool>(type: "bit", nullable: false),
                    ParentTaskId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DeletedUserInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StakeholderId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    TaskCategoryId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletionMode = table.Column<byte>(type: "tinyint", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId1 = table.Column<int>(type: "int", nullable: true),
                    ContractId1 = table.Column<int>(type: "int", nullable: true),
                    TaskCategoryId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_Branch_Tbl_BranchId1",
                        column: x => x.BranchId1,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_Contract_Tbl_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contract_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_Contract_Tbl_ContractId1",
                        column: x => x.ContractId1,
                        principalTable: "Contract_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_Organization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_TaskCategory_Tbl_TaskCategoryId",
                        column: x => x.TaskCategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_TaskCategory_Tbl_TaskCategoryId1",
                        column: x => x.TaskCategoryId1,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_TaskSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_Tasks_Tbl_ParentTaskId",
                        column: x => x.ParentTaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskScheduleAssignment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignmentType = table.Column<byte>(type: "tinyint", nullable: false),
                    CopyDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PredefinedCopyDescriptionId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaskScheduleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskScheduleAssignment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_Tbl_PredefinedCopyDescription_Tbl_PredefinedCopyDescriptionId",
                        column: x => x.PredefinedCopyDescriptionId,
                        principalTable: "PredefinedCopyDescription_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_Tbl_TaskSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_Tbl_TaskSchedule_Tbl_TaskScheduleId",
                        column: x => x.TaskScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskScheduleViewer_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskScheduleViewer_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskScheduleViewer_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskScheduleViewer_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskScheduleViewer_Tbl_TaskSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmploymentType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsSupervisor = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentMember_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentMember_Tbl_Contact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DepartmentMember_Tbl_DepartmentPosition_Tbl_PositionId",
                        column: x => x.PositionId,
                        principalTable: "DepartmentPosition_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DepartmentMember_Tbl_OrganizationDepartment_Tbl_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "OrganizationDepartment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ActivityAttachment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploaderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityAttachment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityAttachment_Tbl_ActivityBase_Tbl_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "ActivityBase_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityAttachment_Tbl_AspNetUsers_UploaderUserId",
                        column: x => x.UploaderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityComment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    ActivityCommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityComment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityComment_Tbl_ActivityBase_Tbl_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "ActivityBase_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityComment_Tbl_ActivityComment_Tbl_ActivityCommentId",
                        column: x => x.ActivityCommentId,
                        principalTable: "ActivityComment_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActivityComment_Tbl_ActivityComment_Tbl_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "ActivityComment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityComment_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityHistory_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityHistory_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityHistory_Tbl_ActivityBase_Tbl_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "ActivityBase_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityHistory_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityCRM_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    CRMId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCRM_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityCRM_Tbl_ActivityBase_Tbl_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "ActivityBase_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityCRM_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityCRM_Tbl_CRMInteraction_Tbl_CRMId",
                        column: x => x.CRMId,
                        principalTable: "CRMInteraction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMAttachment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CRMInteractionId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploaderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMAttachment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMAttachment_Tbl_AspNetUsers_UploaderUserId",
                        column: x => x.UploaderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMAttachment_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                        column: x => x.CRMInteractionId,
                        principalTable: "CRMInteraction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMComment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CRMInteractionId = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    CRMCommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMComment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMComment_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMComment_Tbl_CRMComment_Tbl_CRMCommentId",
                        column: x => x.CRMCommentId,
                        principalTable: "CRMComment_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CRMComment_Tbl_CRMComment_Tbl_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "CRMComment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMComment_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                        column: x => x.CRMInteractionId,
                        principalTable: "CRMInteraction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMParticipant_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CRMInteractionId = table.Column<int>(type: "int", nullable: false),
                    ParticipantType = table.Column<byte>(type: "tinyint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StakeholderContactId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMParticipant_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMParticipant_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMParticipant_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                        column: x => x.CRMInteractionId,
                        principalTable: "CRMInteraction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMParticipant_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                        column: x => x.StakeholderContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMTeam_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CRMInteractionId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    AccessType = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMTeam_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMTeam_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMTeam_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                        column: x => x.CRMInteractionId,
                        principalTable: "CRMInteraction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMTeam_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderOrganizationPosition_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PowerLevel = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderOrganizationPosition_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationPosition_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationPosition_Tbl_StakeholderOrganization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "StakeholderOrganization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityTask_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTask_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTask_Tbl_ActivityBase_Tbl_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "ActivityBase_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityTask_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityTask_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskAssignment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    AssignedUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignedTeamId = table.Column<int>(type: "int", nullable: true),
                    AssignerUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DeletedAssignedUserInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAssignerUserInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignmentType = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PredefinedCopyDescriptionId = table.Column<int>(type: "int", nullable: true),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonalStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonalEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonalTimeNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PersonalDatesUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    IsMyDay = table.Column<bool>(type: "bit", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserReport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedInTeamId = table.Column<int>(type: "int", nullable: true),
                    IsFocused = table.Column<bool>(type: "bit", nullable: false),
                    FocusedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PredefinedCopyDescriptionId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAssignment_Tbl_AspNetUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAssignment_Tbl_AspNetUsers_AssignerUserId",
                        column: x => x.AssignerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAssignment_Tbl_PredefinedCopyDescription_Tbl_PredefinedCopyDescriptionId",
                        column: x => x.PredefinedCopyDescriptionId,
                        principalTable: "PredefinedCopyDescription_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAssignment_Tbl_PredefinedCopyDescription_Tbl_PredefinedCopyDescriptionId1",
                        column: x => x.PredefinedCopyDescriptionId1,
                        principalTable: "PredefinedCopyDescription_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskAssignment_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAssignment_Tbl_Team_Tbl_AssignedInTeamId",
                        column: x => x.AssignedInTeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskAssignment_Tbl_Team_Tbl_AssignedTeamId",
                        column: x => x.AssignedTeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskAttachment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileUUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploaderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TasksId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAttachment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAttachment_Tbl_AspNetUsers_UploaderUserId",
                        column: x => x.UploaderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAttachment_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAttachment_Tbl_Tasks_Tbl_TasksId",
                        column: x => x.TasksId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskComment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    IsImportant = table.Column<bool>(type: "bit", nullable: false),
                    CommentType = table.Column<byte>(type: "tinyint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    EditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskComment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskComment_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskComment_Tbl_TaskComment_Tbl_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "TaskComment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskComment_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskCRMDetails_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    StakeholderContactId = table.Column<int>(type: "int", nullable: true),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    Result = table.Column<byte>(type: "tinyint", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextFollowUpNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCRMDetails_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCRMDetails_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                        column: x => x.StakeholderContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskCRMDetails_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskHistory_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActionType = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RelatedItemId = table.Column<int>(type: "int", nullable: true),
                    RelatedItemType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskHistory_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskHistory_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskHistory_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskOperation_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationOrder = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsStarred = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CompletionNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskOperation_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskOperation_Tbl_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskOperation_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskOperation_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskReminderSchedule_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReminderType = table.Column<byte>(type: "tinyint", nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: true),
                    DaysBeforeDeadline = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificationTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "bit", nullable: false),
                    LastExecuted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReminderSchedule_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskReminderSchedule_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskReminderSchedule_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskScheduleExecution",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    CreatedTaskId = table.Column<int>(type: "int", nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutionDuration = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskScheduleExecution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskScheduleExecution_TaskSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskScheduleExecution_Tasks_Tbl_CreatedTaskId",
                        column: x => x.CreatedTaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskViewer_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessType = table.Column<byte>(type: "tinyint", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    SpecialPermissionType = table.Column<byte>(type: "tinyint", nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsViewed = table.Column<bool>(type: "bit", nullable: false),
                    ViewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TasksId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskViewer_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskViewer_Tbl_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskViewer_Tbl_AspNetUsers_LastUpdaterUserId",
                        column: x => x.LastUpdaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskViewer_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskViewer_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskViewer_Tbl_Tasks_Tbl_TasksId",
                        column: x => x.TasksId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskViewer_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskWorkLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskWorkLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskWorkLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskWorkLog_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderOrganizationMember_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    IsSupervisor = table.Column<bool>(type: "bit", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderOrganizationMember_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationMember_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationMember_Tbl_StakeholderContact_Tbl_ContactId",
                        column: x => x.ContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationMember_Tbl_StakeholderOrganizationPosition_Tbl_PositionId",
                        column: x => x.PositionId,
                        principalTable: "StakeholderOrganizationPosition_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderOrganizationMember_Tbl_StakeholderOrganization_Tbl_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "StakeholderOrganization_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskMyDay_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskAssignmentId = table.Column<int>(type: "int", nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WorkStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WorkDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    RemovedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskMyDay_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskMyDay_Tbl_TaskAssignment_Tbl_TaskAssignmentId",
                        column: x => x.TaskAssignmentId,
                        principalTable: "TaskAssignment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskCommentAttachment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskCommentId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileUUID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploaderUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCommentAttachment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCommentAttachment_Tbl_AspNetUsers_UploaderUserId",
                        column: x => x.UploaderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskCommentAttachment_Tbl_TaskComment_Tbl_TaskCommentId",
                        column: x => x.TaskCommentId,
                        principalTable: "TaskComment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskCommentMention_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    MentionedUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MentionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TaskCommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCommentMention_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCommentMention_Tbl_AspNetUsers_MentionedUserId",
                        column: x => x.MentionedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskCommentMention_Tbl_TaskComment_Tbl_CommentId",
                        column: x => x.CommentId,
                        principalTable: "TaskComment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskCommentMention_Tbl_TaskComment_Tbl_TaskCommentId",
                        column: x => x.TaskCommentId,
                        principalTable: "TaskComment_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskNotification_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: true),
                    CommentId = table.Column<int>(type: "int", nullable: true),
                    OperationId = table.Column<int>(type: "int", nullable: true),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NotificationType = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TaskCommentId = table.Column<int>(type: "int", nullable: true),
                    TasksId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskNotification_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskNotification_Tbl_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskNotification_Tbl_TaskComment_Tbl_CommentId",
                        column: x => x.CommentId,
                        principalTable: "TaskComment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskNotification_Tbl_TaskComment_Tbl_TaskCommentId",
                        column: x => x.TaskCommentId,
                        principalTable: "TaskComment_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskNotification_Tbl_TaskOperation_Tbl_OperationId",
                        column: x => x.OperationId,
                        principalTable: "TaskOperation_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskNotification_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskNotification_Tbl_Tasks_Tbl_TasksId",
                        column: x => x.TasksId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskOperationWorkLog_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskOperationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskOperationWorkLog_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskOperationWorkLog_Tbl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskOperationWorkLog_Tbl_TaskOperation_Tbl_TaskOperationId",
                        column: x => x.TaskOperationId,
                        principalTable: "TaskOperation_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskReminderEvent_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EventType = table.Column<byte>(type: "tinyint", nullable: false),
                    ScheduledDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    SentDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    NotificationChannel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReminderEvent_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskReminderEvent_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskReminderEvent_Tbl_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskReminderEvent_Tbl_TaskReminderSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskReminderSchedule_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskReminderEvent_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName", "RoleLevel" },
                values: new object[,]
                {
                    { "1", "8e446cc7-743a-4133-8241-0f374fcbbc0d", "مدیر سیستم", "Admin", "ADMIN", "1" },
                    { "2", "5b6877d1-6fe6-4f8c-92a4-33fdf65a391f", "مدیر", "Manager", "MANAGER", "2" },
                    { "3", "8f4cee96-4bf9-4019-b589-4de5c0230e2c", "سرپرست", "Supervisor", "SUPERVISOR", "3" },
                    { "4", "523c9ab5-4b4c-43e2-84be-12c4b6f74eed", "کارمند", "Employee", "EMPLOYEE", "4" },
                    { "5", "aa5d01a0-a905-44ef-9e53-9c694828dbff", "کاربر عادی", "User", "USER", "5" }
                });

            migrationBuilder.InsertData(
                table: "Branch_Tbl",
                columns: new[] { "Id", "Address", "BranchId", "CreateDate", "Description", "Email", "IsActive", "IsMainBranch", "LastUpdateDate", "ManagerName", "Name", "ParentId", "Phone", "TelegramBotToken", "TelegramBotTokenName" },
                values: new object[] { 1, null, null, new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "شعبه برند رسنا", null, true, true, null, null, "شعبه رسنا", null, null, null, null });

            migrationBuilder.InsertData(
                table: "PredefinedCopyDescription_Tbl",
                columns: new[] { "Id", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "جهت اطلاع و پیگیری", true, "جهت اطلاع" },
                    { 2, "جهت انجام اقدامات لازم", true, "جهت اقدام" },
                    { 3, "جهت بررسی و اعلام نظر", true, "جهت بررسی" },
                    { 4, "جهت تایید و ابلاغ", true, "جهت تایید" },
                    { 5, "جهت نظارت و کنترل", true, "جهت نظارت" },
                    { 6, "جهت هماهنگی‌های لازم", true, "جهت هماهنگی" },
                    { 7, "جهت پیگیری و گزارش", true, "جهت پیگیری" },
                    { 8, "جهت اجرای دستورات", true, "جهت اجرا" }
                });

            migrationBuilder.InsertData(
                table: "RolePattern_Tbl",
                columns: new[] { "Id", "AccessLevel", "CreateDate", "CreatorUserId", "Description", "IsActive", "IsSystemPattern", "LastUpdateDate", "LastUpdaterUserId", "PatternName" },
                values: new object[,]
                {
                    { 1, (byte)1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "دسترسی کامل به تمام بخش‌ها", true, true, null, null, "مدیریت کامل" },
                    { 2, (byte)2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "مدیریت عملیات و تسک‌ها", true, true, null, null, "مدیر عملیات" },
                    { 3, (byte)4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "دسترسی به ماژول فروش و CRM", true, true, null, null, "کارشناس فروش" },
                    { 4, (byte)5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "دسترسی محدود به تسک‌های شخصی", true, true, null, null, "کاربر عادی" }
                });

            migrationBuilder.InsertData(
                table: "TaskCategory_Tbl",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "ParentCategoryId", "TaskCategoryId", "Title" },
                values: new object[,]
                {
                    { 1, "دسته‌بندی عمومی برای تسک‌ها", (byte)1, true, null, null, "عمومی" },
                    { 2, "تسک‌های مربوط به امور اداری", (byte)2, true, null, null, "اداری" },
                    { 3, "تسک‌های مربوط به فروش", (byte)4, true, null, null, "فروش" },
                    { 4, "تسک‌های مربوط به خدمات مشتریان غیر حضوری", (byte)5, true, null, null, "خدمات حضوری" },
                    { 5, "تسک‌های مربوط به خدمات مشتریان حضوری", (byte)5, true, null, null, "خدمات  غیر حضوری" },
                    { 6, "تسک‌های بازاریابی و تبلیغات", (byte)6, true, null, null, "بازاریابی" },
                    { 7, "تسک‌های مربوط به امور مالی", (byte)7, true, null, null, "مالی" },
                    { 8, "تسک‌های مربوط به HR", (byte)8, true, null, null, "منابع انسانی" },
                    { 9, "تسک‌های فوری و اضطراری", (byte)10, true, null, null, "فوری" }
                });

            migrationBuilder.InsertData(
                table: "RolePatternDetails_Tbl",
                columns: new[] { "Id", "ActionName", "CanApprove", "CanCreate", "CanDelete", "CanEdit", "CanRead", "ControllerName", "DataAccessLevel", "IsActive", "RolePatternId" },
                values: new object[,]
                {
                    { 1, "*", true, true, true, true, true, "Tasks", (byte)2, true, 1 },
                    { 2, "*", true, true, true, true, true, "CRM", (byte)2, true, 1 },
                    { 3, "*", true, true, true, true, true, "Stakeholder", (byte)2, true, 1 },
                    { 4, "*", true, true, true, true, true, "Contract", (byte)2, true, 1 },
                    { 5, "*", true, true, true, true, true, "User", (byte)2, true, 1 },
                    { 6, "*", true, true, true, true, true, "RolePattern", (byte)2, true, 1 },
                    { 7, "*", true, true, true, true, true, "UserPermission", (byte)2, true, 1 },
                    { 8, "*", true, true, true, true, true, "Branch", (byte)2, true, 1 },
                    { 9, "*", true, true, true, true, true, "Team", (byte)2, true, 1 },
                    { 10, "*", true, true, true, true, true, "TaskCategory", (byte)2, true, 1 },
                    { 11, "*", true, true, true, true, true, "Tasks", (byte)1, true, 2 },
                    { 12, "Index,Details,Create,Edit", false, true, false, true, true, "CRM", (byte)1, true, 2 },
                    { 13, "Index,Details", false, false, false, false, true, "Stakeholder", (byte)1, true, 2 },
                    { 14, "*", false, true, false, true, true, "CRM", (byte)0, true, 3 },
                    { 15, "Index,Details,Create,Edit", false, true, false, true, true, "Stakeholder", (byte)0, true, 3 },
                    { 16, "Index,Details,MyTasks", false, false, false, false, true, "Tasks", (byte)0, true, 3 },
                    { 17, "Index,Details,MyTasks", false, false, false, false, true, "Tasks", (byte)0, true, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityAttachment_Tbl_ActivityId",
                table: "ActivityAttachment_Tbl",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityAttachment_Tbl_UploaderUserId",
                table: "ActivityAttachment_Tbl",
                column: "UploaderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_Tbl_BranchId",
                table: "ActivityBase_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_Tbl_ContractId",
                table: "ActivityBase_Tbl",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_Tbl_CreatorUserId",
                table: "ActivityBase_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_Tbl_LastUpdaterUserId",
                table: "ActivityBase_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBase_Tbl_StakeholderId",
                table: "ActivityBase_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityComment_Tbl_ActivityCommentId",
                table: "ActivityComment_Tbl",
                column: "ActivityCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityComment_Tbl_ActivityId",
                table: "ActivityComment_Tbl",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityComment_Tbl_CreatorUserId",
                table: "ActivityComment_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityComment_Tbl_ParentCommentId",
                table: "ActivityComment_Tbl",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCRM_Tbl_ActivityId",
                table: "ActivityCRM_Tbl",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCRM_Tbl_CreatorUserId",
                table: "ActivityCRM_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCRM_Tbl_CRMId",
                table: "ActivityCRM_Tbl",
                column: "CRMId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHistory_Tbl_ActivityId",
                table: "ActivityHistory_Tbl",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHistory_Tbl_CreatorUserId",
                table: "ActivityHistory_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTask_Tbl_ActivityId",
                table: "ActivityTask_Tbl",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTask_Tbl_CreatorUserId",
                table: "ActivityTask_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTask_Tbl_TaskId",
                table: "ActivityTask_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DirectManagerUserId",
                table: "AspNetUsers",
                column: "DirectManagerUserId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Tbl_BranchId",
                table: "Branch_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Tbl_ParentId",
                table: "Branch_Tbl",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContact_Branch_Contact",
                table: "BranchContact_Tbl",
                columns: new[] { "BranchId", "ContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchContact_Tbl_AssignedByUserId",
                table: "BranchContact_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContact_Tbl_ContactId",
                table: "BranchContact_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroup_Branch_Code",
                table: "BranchContactGroup_Tbl",
                columns: new[] { "BranchId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroup_Tbl_CreatorUserId",
                table: "BranchContactGroup_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroup_Tbl_LastUpdaterUserId",
                table: "BranchContactGroup_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroupMember_Group_Contact",
                table: "BranchContactGroupMember_Tbl",
                columns: new[] { "BranchGroupId", "BranchContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroupMember_Tbl_AddedByUserId",
                table: "BranchContactGroupMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchContactGroupMember_Tbl_BranchContactId",
                table: "BranchContactGroupMember_Tbl",
                column: "BranchContactId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchModulePermission_Tbl_BranchId",
                table: "BranchModulePermission_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchModulePermission_Tbl_GrantedByUserId",
                table: "BranchModulePermission_Tbl",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganization_Branch_Organization",
                table: "BranchOrganization_Tbl",
                columns: new[] { "BranchId", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganization_Tbl_AssignedByUserId",
                table: "BranchOrganization_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganization_Tbl_OrganizationId",
                table: "BranchOrganization_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroup_Tbl_BranchId",
                table: "BranchOrganizationGroup_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroup_Tbl_CreatorUserId",
                table: "BranchOrganizationGroup_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroup_Tbl_LastUpdaterUserId",
                table: "BranchOrganizationGroup_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroupMember_Group_Organization",
                table: "BranchOrganizationGroupMember_Tbl",
                columns: new[] { "BranchGroupId", "BranchOrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroupMember_Tbl_AddedByUserId",
                table: "BranchOrganizationGroupMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchOrganizationGroupMember_Tbl_BranchOrganizationId",
                table: "BranchOrganizationGroupMember_Tbl",
                column: "BranchOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategoryStakeholder_Tbl_AssignedByUserId",
                table: "BranchTaskCategoryStakeholder_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategoryStakeholder_Tbl_BranchId",
                table: "BranchTaskCategoryStakeholder_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategoryStakeholder_Tbl_StakeholderId",
                table: "BranchTaskCategoryStakeholder_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategoryStakeholder_Tbl_TaskCategoryId",
                table: "BranchTaskCategoryStakeholder_Tbl",
                column: "TaskCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTaskCategoryStakeholder_Tbl_TaskCategoryId1",
                table: "BranchTaskCategoryStakeholder_Tbl",
                column: "TaskCategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_BranchUser_Tbl_AssignedByUserId",
                table: "BranchUser_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchUser_Tbl_BranchId",
                table: "BranchUser_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchUser_Tbl_BranchId1",
                table: "BranchUser_Tbl",
                column: "BranchId1");

            migrationBuilder.CreateIndex(
                name: "IX_BranchUser_Tbl_UserId",
                table: "BranchUser_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_NationalCode",
                table: "Contact_Tbl",
                column: "NationalCode",
                unique: true,
                filter: "[NationalCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_Tbl_CreatorUserId",
                table: "Contact_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_Tbl_LastUpdaterUserId",
                table: "Contact_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroup_Code",
                table: "ContactGroup_Tbl",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroup_Tbl_CreatorUserId",
                table: "ContactGroup_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroup_Tbl_LastUpdaterUserId",
                table: "ContactGroup_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroupMember_Group_Contact",
                table: "ContactGroupMember_Tbl",
                columns: new[] { "GroupId", "ContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroupMember_Tbl_AddedByUserId",
                table: "ContactGroupMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactGroupMember_Tbl_ContactId",
                table: "ContactGroupMember_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhone_Contact_Number",
                table: "ContactPhone_Tbl",
                columns: new[] { "ContactId", "PhoneNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhone_Tbl_CreatorUserId",
                table: "ContactPhone_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Tbl_CreatorUserId",
                table: "Contract_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Tbl_LastUpdaterUserId",
                table: "Contract_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Tbl_StakeholderId",
                table: "Contract_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_CoreNotification_Tbl_BranchId",
                table: "CoreNotification_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CoreNotification_Tbl_RecipientUserId",
                table: "CoreNotification_Tbl",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoreNotification_Tbl_SenderUserId",
                table: "CoreNotification_Tbl",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoreNotificationDelivery_Tbl_CoreNotificationId",
                table: "CoreNotificationDelivery_Tbl",
                column: "CoreNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CoreNotificationDetail_Tbl_CoreNotificationId",
                table: "CoreNotificationDetail_Tbl",
                column: "CoreNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CoreNotificationSetting_Tbl_UserId",
                table: "CoreNotificationSetting_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMAttachment_Tbl_CRMInteractionId",
                table: "CRMAttachment_Tbl",
                column: "CRMInteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMAttachment_Tbl_UploaderUserId",
                table: "CRMAttachment_Tbl",
                column: "UploaderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMComment_Tbl_CreatorUserId",
                table: "CRMComment_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMComment_Tbl_CRMCommentId",
                table: "CRMComment_Tbl",
                column: "CRMCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMComment_Tbl_CRMInteractionId",
                table: "CRMComment_Tbl",
                column: "CRMInteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMComment_Tbl_ParentCommentId",
                table: "CRMComment_Tbl",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMInteraction_Tbl_BranchId",
                table: "CRMInteraction_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMInteraction_Tbl_ContractId",
                table: "CRMInteraction_Tbl",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMInteraction_Tbl_CreatorUserId",
                table: "CRMInteraction_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMInteraction_Tbl_LastUpdaterUserId",
                table: "CRMInteraction_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMInteraction_Tbl_StakeholderContactId",
                table: "CRMInteraction_Tbl",
                column: "StakeholderContactId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMInteraction_Tbl_StakeholderId",
                table: "CRMInteraction_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMParticipant_Tbl_CRMInteractionId",
                table: "CRMParticipant_Tbl",
                column: "CRMInteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMParticipant_Tbl_StakeholderContactId",
                table: "CRMParticipant_Tbl",
                column: "StakeholderContactId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMParticipant_Tbl_UserId",
                table: "CRMParticipant_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMTeam_Tbl_CreatorUserId",
                table: "CRMTeam_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMTeam_Tbl_CRMInteractionId",
                table: "CRMTeam_Tbl",
                column: "CRMInteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMTeam_Tbl_TeamId",
                table: "CRMTeam_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMember_Department_Contact",
                table: "DepartmentMember_Tbl",
                columns: new[] { "DepartmentId", "ContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMember_Tbl_ContactId",
                table: "DepartmentMember_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMember_Tbl_CreatorUserId",
                table: "DepartmentMember_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMember_Tbl_PositionId",
                table: "DepartmentMember_Tbl",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentPosition_Tbl_CreatorUserId",
                table: "DepartmentPosition_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentPosition_Tbl_DepartmentId",
                table: "DepartmentPosition_Tbl",
                column: "DepartmentId");

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
                name: "IX_Organization_EconomicCode",
                table: "Organization_Tbl",
                column: "EconomicCode",
                unique: true,
                filter: "[EconomicCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_RegistrationNumber",
                table: "Organization_Tbl",
                column: "RegistrationNumber",
                unique: true,
                filter: "[RegistrationNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Tbl_CreatorUserId",
                table: "Organization_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Tbl_LastUpdaterUserId",
                table: "Organization_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationContact_Org_Contact_Type",
                table: "OrganizationContact_Tbl",
                columns: new[] { "OrganizationId", "ContactId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationContact_Tbl_ContactId",
                table: "OrganizationContact_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationContact_Tbl_CreatorUserId",
                table: "OrganizationContact_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_CreatorUserId",
                table: "OrganizationDepartment_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_LastUpdaterUserId",
                table: "OrganizationDepartment_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_ManagerContactId",
                table: "OrganizationDepartment_Tbl",
                column: "ManagerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_OrganizationId",
                table: "OrganizationDepartment_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartment_Tbl_ParentDepartmentId",
                table: "OrganizationDepartment_Tbl",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroup_Code",
                table: "OrganizationGroup_Tbl",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroup_Tbl_CreatorUserId",
                table: "OrganizationGroup_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroup_Tbl_LastUpdaterUserId",
                table: "OrganizationGroup_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroupMember_Group_Organization",
                table: "OrganizationGroupMember_Tbl",
                columns: new[] { "GroupId", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroupMember_Tbl_AddedByUserId",
                table: "OrganizationGroupMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroupMember_Tbl_OrganizationId",
                table: "OrganizationGroupMember_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Tbl_CreatorId",
                table: "Permission_Tbl",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Tbl_LastUpdaterId",
                table: "Permission_Tbl",
                column: "LastUpdaterId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Tbl_ParentId",
                table: "Permission_Tbl",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionChangeLog_Tbl_ChangedByUserId",
                table: "PermissionChangeLog_Tbl",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionChangeLog_Tbl_PermissionId",
                table: "PermissionChangeLog_Tbl",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionChangeLog_Tbl_UserId",
                table: "PermissionChangeLog_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionLog_Tbl_UserId",
                table: "PermissionLog_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Tbl_CreatorId",
                table: "Role_Tbl",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Tbl_LastUpdaterId",
                table: "Role_Tbl",
                column: "LastUpdaterId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePattern_PatternName",
                table: "RolePattern_Tbl",
                column: "PatternName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePattern_Tbl_CreatorUserId",
                table: "RolePattern_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePattern_Tbl_LastUpdaterUserId",
                table: "RolePattern_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePatternDetails_Tbl_RolePatternId",
                table: "RolePatternDetails_Tbl",
                column: "RolePatternId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_Tbl_AssignedByUserId",
                table: "RolePermission_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_Tbl_PermissionId",
                table: "RolePermission_Tbl",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_Tbl_RoleId",
                table: "RolePermission_Tbl",
                column: "RoleId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Stakeholder_Tbl_BranchId",
                table: "Stakeholder_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Stakeholder_Tbl_CreatorUserId",
                table: "Stakeholder_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Stakeholder_Tbl_LastUpdaterUserId",
                table: "Stakeholder_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderBranch_Tbl_AssignedByUserId",
                table: "StakeholderBranch_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderBranch_Tbl_BranchId",
                table: "StakeholderBranch_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderBranch_Tbl_CreatorUserId",
                table: "StakeholderBranch_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderBranch_Tbl_StakeholderId",
                table: "StakeholderBranch_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderBranch_Tbl_StakeholderId1",
                table: "StakeholderBranch_Tbl",
                column: "StakeholderId1");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderContact_Tbl_CreatorUserId",
                table: "StakeholderContact_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderContact_Tbl_StakeholderId",
                table: "StakeholderContact_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderCRM_Tbl_SalesRepUserId",
                table: "StakeholderCRM_Tbl",
                column: "SalesRepUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderCRM_Tbl_StakeholderId",
                table: "StakeholderCRM_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_CreatorUserId",
                table: "StakeholderOrganization_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_LastUpdaterUserId",
                table: "StakeholderOrganization_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_ManagerContactId",
                table: "StakeholderOrganization_Tbl",
                column: "ManagerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_ParentOrganizationId",
                table: "StakeholderOrganization_Tbl",
                column: "ParentOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganization_Tbl_StakeholderId",
                table: "StakeholderOrganization_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationMember_Tbl_ContactId",
                table: "StakeholderOrganizationMember_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationMember_Tbl_CreatorUserId",
                table: "StakeholderOrganizationMember_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationMember_Tbl_OrganizationId",
                table: "StakeholderOrganizationMember_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationMember_Tbl_PositionId",
                table: "StakeholderOrganizationMember_Tbl",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationPosition_Tbl_CreatorUserId",
                table: "StakeholderOrganizationPosition_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderOrganizationPosition_Tbl_OrganizationId",
                table: "StakeholderOrganizationPosition_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignment_Tbl_AssignedInTeamId",
                table: "TaskAssignment_Tbl",
                column: "AssignedInTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignment_Tbl_AssignedTeamId",
                table: "TaskAssignment_Tbl",
                column: "AssignedTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignment_Tbl_AssignedUserId",
                table: "TaskAssignment_Tbl",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignment_Tbl_AssignerUserId",
                table: "TaskAssignment_Tbl",
                column: "AssignerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignment_Tbl_PredefinedCopyDescriptionId",
                table: "TaskAssignment_Tbl",
                column: "PredefinedCopyDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignment_Tbl_PredefinedCopyDescriptionId1",
                table: "TaskAssignment_Tbl",
                column: "PredefinedCopyDescriptionId1");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignment_Tbl_TaskId",
                table: "TaskAssignment_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAttachment_Tbl_TaskId",
                table: "TaskAttachment_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAttachment_Tbl_TasksId",
                table: "TaskAttachment_Tbl",
                column: "TasksId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAttachment_Tbl_UploaderUserId",
                table: "TaskAttachment_Tbl",
                column: "UploaderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategory_Tbl_ParentCategoryId",
                table: "TaskCategory_Tbl",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategory_Tbl_TaskCategoryId",
                table: "TaskCategory_Tbl",
                column: "TaskCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComment_Tbl_CreatorUserId",
                table: "TaskComment_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComment_Tbl_ParentCommentId",
                table: "TaskComment_Tbl",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComment_Tbl_TaskId",
                table: "TaskComment_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCommentAttachment_Tbl_TaskCommentId",
                table: "TaskCommentAttachment_Tbl",
                column: "TaskCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCommentAttachment_Tbl_UploaderUserId",
                table: "TaskCommentAttachment_Tbl",
                column: "UploaderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCommentMention_Tbl_CommentId",
                table: "TaskCommentMention_Tbl",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCommentMention_Tbl_MentionedUserId",
                table: "TaskCommentMention_Tbl",
                column: "MentionedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCommentMention_Tbl_TaskCommentId",
                table: "TaskCommentMention_Tbl",
                column: "TaskCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCRMDetails_Tbl_StakeholderContactId",
                table: "TaskCRMDetails_Tbl",
                column: "StakeholderContactId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCRMDetails_Tbl_TaskId",
                table: "TaskCRMDetails_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistory_Tbl_TaskId",
                table: "TaskHistory_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistory_Tbl_UserId",
                table: "TaskHistory_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMyDay_Assignment_Date",
                table: "TaskMyDay_Tbl",
                columns: new[] { "TaskAssignmentId", "PlannedDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskNotification_Tbl_CommentId",
                table: "TaskNotification_Tbl",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskNotification_Tbl_OperationId",
                table: "TaskNotification_Tbl",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskNotification_Tbl_RecipientUserId",
                table: "TaskNotification_Tbl",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskNotification_Tbl_TaskCommentId",
                table: "TaskNotification_Tbl",
                column: "TaskCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskNotification_Tbl_TaskId",
                table: "TaskNotification_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskNotification_Tbl_TasksId",
                table: "TaskNotification_Tbl",
                column: "TasksId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskOperation_Tbl_CompletedByUserId",
                table: "TaskOperation_Tbl",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskOperation_Tbl_CreatorUserId",
                table: "TaskOperation_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskOperation_Tbl_TaskId",
                table: "TaskOperation_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskOperationWorkLog_Tbl_TaskOperationId",
                table: "TaskOperationWorkLog_Tbl",
                column: "TaskOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskOperationWorkLog_Tbl_UserId",
                table: "TaskOperationWorkLog_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderEvent_Tbl_CreatorUserId",
                table: "TaskReminderEvent_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderEvent_Tbl_RecipientUserId",
                table: "TaskReminderEvent_Tbl",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderEvent_Tbl_ScheduleId",
                table: "TaskReminderEvent_Tbl",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderEvent_Tbl_TaskId",
                table: "TaskReminderEvent_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderSchedule_Tbl_CreatorUserId",
                table: "TaskReminderSchedule_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminderSchedule_Tbl_TaskId",
                table: "TaskReminderSchedule_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_BranchId",
                table: "Tasks_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_BranchId1",
                table: "Tasks_Tbl",
                column: "BranchId1");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_ContactId",
                table: "Tasks_Tbl",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_ContractId",
                table: "Tasks_Tbl",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_ContractId1",
                table: "Tasks_Tbl",
                column: "ContractId1");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_CreatorUserId",
                table: "Tasks_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_OrganizationId",
                table: "Tasks_Tbl",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_ParentTaskId",
                table: "Tasks_Tbl",
                column: "ParentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_ScheduleId",
                table: "Tasks_Tbl",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_StakeholderId",
                table: "Tasks_Tbl",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_TaskCategoryId",
                table: "Tasks_Tbl",
                column: "TaskCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_TaskCategoryId1",
                table: "Tasks_Tbl",
                column: "TaskCategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_TeamId",
                table: "Tasks_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedule_Tbl_CreatorUserId",
                table: "TaskSchedule_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedule_Tbl_ModifierUserId",
                table: "TaskSchedule_Tbl",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedule_Tbl_TaskTemplateId",
                table: "TaskSchedule_Tbl",
                column: "TaskTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedule_Tbl_TaskTemplateId1",
                table: "TaskSchedule_Tbl",
                column: "TaskTemplateId1");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_Tbl_CreatorUserId",
                table: "TaskScheduleAssignment_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_Tbl_PredefinedCopyDescriptionId",
                table: "TaskScheduleAssignment_Tbl",
                column: "PredefinedCopyDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_Tbl_ScheduleId",
                table: "TaskScheduleAssignment_Tbl",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_Tbl_TaskScheduleId",
                table: "TaskScheduleAssignment_Tbl",
                column: "TaskScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_Tbl_UserId",
                table: "TaskScheduleAssignment_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleExecution_CreatedTaskId",
                table: "TaskScheduleExecution",
                column: "CreatedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleExecution_ScheduleId",
                table: "TaskScheduleExecution",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleViewer_Tbl_AddedByUserId",
                table: "TaskScheduleViewer_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleViewer_Tbl_ScheduleId",
                table: "TaskScheduleViewer_Tbl",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleViewer_Tbl_UserId",
                table: "TaskScheduleViewer_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplate_Tbl_CategoryId",
                table: "TaskTemplate_Tbl",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplate_Tbl_CreatorUserId",
                table: "TaskTemplate_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplateOperation_Tbl_TaskTemplateId",
                table: "TaskTemplateOperation_Tbl",
                column: "TaskTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplateOperation_Tbl_TemplateId",
                table: "TaskTemplateOperation_Tbl",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_AddedByUserId",
                table: "TaskViewer_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_LastUpdaterUserId",
                table: "TaskViewer_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_TaskId",
                table: "TaskViewer_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_TasksId",
                table: "TaskViewer_Tbl",
                column: "TasksId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_TeamId",
                table: "TaskViewer_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_UserId",
                table: "TaskViewer_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_AddedByUserId",
                table: "TaskViewPermission_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_GranteeUserId",
                table: "TaskViewPermission_Tbl",
                column: "GranteeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_LastUpdaterUserId",
                table: "TaskViewPermission_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_TargetTeamId",
                table: "TaskViewPermission_Tbl",
                column: "TargetTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_TargetUserId",
                table: "TaskViewPermission_Tbl",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewPermission_Tbl_TeamId",
                table: "TaskViewPermission_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkLog_Tbl_TaskId",
                table: "TaskWorkLog_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkLog_Tbl_UserId",
                table: "TaskWorkLog_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_Tbl_BranchId",
                table: "Team_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_Tbl_CreatorUserId",
                table: "Team_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_Tbl_LastUpdaterUserId",
                table: "Team_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_Tbl_ManagerUserId",
                table: "Team_Tbl",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_Tbl_ParentTeamId",
                table: "Team_Tbl",
                column: "ParentTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_Tbl_TeamId",
                table: "Team_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_Tbl_AddedByUserId",
                table: "TeamMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_Tbl_PositionId",
                table: "TeamMember_Tbl",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_Tbl_TeamId",
                table: "TeamMember_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_Tbl_UserId",
                table: "TeamMember_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamModulePermission_Tbl_GrantedByUserId",
                table: "TeamModulePermission_Tbl",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamModulePermission_Tbl_TeamId",
                table: "TeamModulePermission_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPosition_Tbl_CreatorUserId",
                table: "TeamPosition_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPosition_Tbl_LastUpdaterUserId",
                table: "TeamPosition_Tbl",
                column: "LastUpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPosition_Tbl_TeamId",
                table: "TeamPosition_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLog_Tbl_BranchId",
                table: "UserActivityLog_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLog_Tbl_UserId",
                table: "UserActivityLog_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePermission_Tbl_GrantedByUserId",
                table: "UserModulePermission_Tbl",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePermission_Tbl_UserId",
                table: "UserModulePermission_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePreference_Tbl_UserId",
                table: "UserModulePreference_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_AssignedByUserId",
                table: "UserPermission_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_ModifiedByUserId",
                table: "UserPermission_Tbl",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_PermissionId",
                table: "UserPermission_Tbl",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_SourceRoleId",
                table: "UserPermission_Tbl",
                column: "SourceRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_Tbl_UserId",
                table: "UserPermission_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Tbl_AssignedByUserId",
                table: "UserRole_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Tbl_RoleId",
                table: "UserRole_Tbl",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Tbl_UserId",
                table: "UserRole_Tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRolePattern_Tbl_AssignedByUserId",
                table: "UserRolePattern_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRolePattern_Tbl_RolePatternId",
                table: "UserRolePattern_Tbl",
                column: "RolePatternId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRolePattern_User_Pattern",
                table: "UserRolePattern_Tbl",
                columns: new[] { "UserId", "RolePatternId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityAttachment_Tbl");

            migrationBuilder.DropTable(
                name: "ActivityComment_Tbl");

            migrationBuilder.DropTable(
                name: "ActivityCRM_Tbl");

            migrationBuilder.DropTable(
                name: "ActivityHistory_Tbl");

            migrationBuilder.DropTable(
                name: "ActivityTask_Tbl");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BranchContactGroupMember_Tbl");

            migrationBuilder.DropTable(
                name: "BranchModulePermission_Tbl");

            migrationBuilder.DropTable(
                name: "BranchOrganizationGroupMember_Tbl");

            migrationBuilder.DropTable(
                name: "BranchTaskCategoryStakeholder_Tbl");

            migrationBuilder.DropTable(
                name: "BranchUser_Tbl");

            migrationBuilder.DropTable(
                name: "ContactGroupMember_Tbl");

            migrationBuilder.DropTable(
                name: "ContactPhone_Tbl");

            migrationBuilder.DropTable(
                name: "CoreNotificationDelivery_Tbl");

            migrationBuilder.DropTable(
                name: "CoreNotificationDetail_Tbl");

            migrationBuilder.DropTable(
                name: "CoreNotificationSetting_Tbl");

            migrationBuilder.DropTable(
                name: "CRMAttachment_Tbl");

            migrationBuilder.DropTable(
                name: "CRMComment_Tbl");

            migrationBuilder.DropTable(
                name: "CRMParticipant_Tbl");

            migrationBuilder.DropTable(
                name: "CRMTeam_Tbl");

            migrationBuilder.DropTable(
                name: "DepartmentMember_Tbl");

            migrationBuilder.DropTable(
                name: "EmailLog_Tbl");

            migrationBuilder.DropTable(
                name: "EmailQueue_Tbl");

            migrationBuilder.DropTable(
                name: "EmailTemplateRecipient_Tbl");

            migrationBuilder.DropTable(
                name: "OrganizationContact_Tbl");

            migrationBuilder.DropTable(
                name: "OrganizationGroupMember_Tbl");

            migrationBuilder.DropTable(
                name: "PermissionChangeLog_Tbl");

            migrationBuilder.DropTable(
                name: "PermissionLog_Tbl");

            migrationBuilder.DropTable(
                name: "RolePatternDetails_Tbl");

            migrationBuilder.DropTable(
                name: "RolePermission_Tbl");

            migrationBuilder.DropTable(
                name: "Settings_Tbl");

            migrationBuilder.DropTable(
                name: "SmsQueue_Tbl");

            migrationBuilder.DropTable(
                name: "SmsTemplateRecipient_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderBranch_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderCRM_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderOrganizationMember_Tbl");

            migrationBuilder.DropTable(
                name: "TaskAttachment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskCommentAttachment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskCommentMention_Tbl");

            migrationBuilder.DropTable(
                name: "TaskCRMDetails_Tbl");

            migrationBuilder.DropTable(
                name: "TaskHistory_Tbl");

            migrationBuilder.DropTable(
                name: "TaskMyDay_Tbl");

            migrationBuilder.DropTable(
                name: "TaskNotification_Tbl");

            migrationBuilder.DropTable(
                name: "TaskOperationWorkLog_Tbl");

            migrationBuilder.DropTable(
                name: "TaskReminderEvent_Tbl");

            migrationBuilder.DropTable(
                name: "TaskScheduleAssignment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskScheduleExecution");

            migrationBuilder.DropTable(
                name: "TaskScheduleViewer_Tbl");

            migrationBuilder.DropTable(
                name: "TaskTemplateOperation_Tbl");

            migrationBuilder.DropTable(
                name: "TaskViewer_Tbl");

            migrationBuilder.DropTable(
                name: "TaskViewPermission_Tbl");

            migrationBuilder.DropTable(
                name: "TaskWorkLog_Tbl");

            migrationBuilder.DropTable(
                name: "TeamMember_Tbl");

            migrationBuilder.DropTable(
                name: "TeamModulePermission_Tbl");

            migrationBuilder.DropTable(
                name: "UserActivityLog_Tbl");

            migrationBuilder.DropTable(
                name: "UserModulePermission_Tbl");

            migrationBuilder.DropTable(
                name: "UserModulePreference_Tbl");

            migrationBuilder.DropTable(
                name: "UserPermission_Tbl");

            migrationBuilder.DropTable(
                name: "UserRole_Tbl");

            migrationBuilder.DropTable(
                name: "UserRolePattern_Tbl");

            migrationBuilder.DropTable(
                name: "ActivityBase_Tbl");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "BranchContactGroup_Tbl");

            migrationBuilder.DropTable(
                name: "BranchContact_Tbl");

            migrationBuilder.DropTable(
                name: "BranchOrganizationGroup_Tbl");

            migrationBuilder.DropTable(
                name: "BranchOrganization_Tbl");

            migrationBuilder.DropTable(
                name: "ContactGroup_Tbl");

            migrationBuilder.DropTable(
                name: "CoreNotification_Tbl");

            migrationBuilder.DropTable(
                name: "CRMInteraction_Tbl");

            migrationBuilder.DropTable(
                name: "DepartmentPosition_Tbl");

            migrationBuilder.DropTable(
                name: "EmailTemplate_Tbl");

            migrationBuilder.DropTable(
                name: "OrganizationGroup_Tbl");

            migrationBuilder.DropTable(
                name: "SmsLog_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderOrganizationPosition_Tbl");

            migrationBuilder.DropTable(
                name: "TaskAssignment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskComment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskOperation_Tbl");

            migrationBuilder.DropTable(
                name: "TaskReminderSchedule_Tbl");

            migrationBuilder.DropTable(
                name: "TeamPosition_Tbl");

            migrationBuilder.DropTable(
                name: "Permission_Tbl");

            migrationBuilder.DropTable(
                name: "Role_Tbl");

            migrationBuilder.DropTable(
                name: "RolePattern_Tbl");

            migrationBuilder.DropTable(
                name: "OrganizationDepartment_Tbl");

            migrationBuilder.DropTable(
                name: "SmsProvider_Tbl");

            migrationBuilder.DropTable(
                name: "SmsTemplate_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderOrganization_Tbl");

            migrationBuilder.DropTable(
                name: "PredefinedCopyDescription_Tbl");

            migrationBuilder.DropTable(
                name: "Tasks_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderContact_Tbl");

            migrationBuilder.DropTable(
                name: "Contact_Tbl");

            migrationBuilder.DropTable(
                name: "Contract_Tbl");

            migrationBuilder.DropTable(
                name: "Organization_Tbl");

            migrationBuilder.DropTable(
                name: "TaskSchedule_Tbl");

            migrationBuilder.DropTable(
                name: "Team_Tbl");

            migrationBuilder.DropTable(
                name: "Stakeholder_Tbl");

            migrationBuilder.DropTable(
                name: "TaskTemplate_Tbl");

            migrationBuilder.DropTable(
                name: "Branch_Tbl");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "TaskCategory_Tbl");
        }
    }
}
