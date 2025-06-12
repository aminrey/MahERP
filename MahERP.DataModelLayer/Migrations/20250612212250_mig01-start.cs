using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig01start : Migration
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
                    DirectManagerUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: false),
                    OrganizationalLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    ParentUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChatidUserTelegram = table.Column<long>(type: "bigint", nullable: true),
                    TelegramRobatId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoveUser = table.Column<bool>(type: "bit", nullable: false),
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
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ManagerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsMainBranch = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                name: "RolePattern_Tbl",
                columns: table => new
                {
                    RolePatternID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolePatternName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RolePatternDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePattern_Tbl", x => x.RolePatternID);
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategory_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCategory_Tbl_TaskCategory_Tbl_ParentCategoryId",
                        column: x => x.ParentCategoryId,
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
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stakeholder_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StakeholderType = table.Column<byte>(type: "tinyint", nullable: false),
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
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ParentTeamId = table.Column<int>(type: "int", nullable: true),
                    ManagerUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    AccessLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Team_Tbl_Team_Tbl_ParentTeamId",
                        column: x => x.ParentTeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolePatternDetails_Tbl",
                columns: table => new
                {
                    RolePatternDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolePatternID = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePatternDetails_Tbl", x => x.RolePatternDetailsID);
                    table.ForeignKey(
                        name: "FK_RolePatternDetails_Tbl_AspNetRoles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolePatternDetails_Tbl_RolePattern_Tbl_RolePatternID",
                        column: x => x.RolePatternID,
                        principalTable: "RolePattern_Tbl",
                        principalColumn: "RolePatternID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplate_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    TaskType = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTemplate_Tbl_TaskCategory_Tbl_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TaskCategory_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contract_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderBranch_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderBranch_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StakeholderBranch_Tbl_Branch_Tbl_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StakeholderBranch_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderContact_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPriority = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderContact_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderContact_Tbl_Stakeholder_Tbl_StakeholderId",
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
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoleDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
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
                        name: "FK_TeamMember_Tbl_Team_Tbl_TeamId",
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
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    LastRunErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplateOperation_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperationOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplateOperation_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTemplateOperation_Tbl_TaskTemplate_Tbl_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "TaskTemplate_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityBase_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModuleType = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StakeholderId = table.Column<int>(type: "int", nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityBase_Tbl_Contract_Tbl_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contract_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActivityBase_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CRMInteraction_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CRMCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CRMType = table.Column<byte>(type: "tinyint", nullable: false),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    Result = table.Column<byte>(type: "tinyint", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeetingLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StakeholderId = table.Column<int>(type: "int", nullable: true),
                    StakeholderContactId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextFollowUpNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdaterUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CRMInteraction_Tbl_Contract_Tbl_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contract_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CRMInteraction_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                        column: x => x.StakeholderContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CRMInteraction_Tbl_Stakeholder_Tbl_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "Stakeholder_Tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tasks_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    TaskType = table.Column<byte>(type: "tinyint", nullable: false),
                    TaskTypeInput = table.Column<byte>(type: "tinyint", nullable: false),
                    VisibilityLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    Important = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupervisorApproved = table.Column<bool>(type: "bit", nullable: true),
                    SupervisorApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerApproved = table.Column<bool>(type: "bit", nullable: true),
                    ManagerApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    CreationMode = table.Column<byte>(type: "tinyint", nullable: false),
                    ParentTaskId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StakeholderId = table.Column<int>(type: "int", nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    TaskCategoryId = table.Column<int>(type: "int", nullable: true),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId1 = table.Column<int>(type: "int", nullable: true)
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
                        name: "FK_Tasks_Tbl_Contract_Tbl_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contract_Tbl",
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskScheduleAssignment",
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
                    table.PrimaryKey("PK_TaskScheduleAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_PredefinedCopyDescription_Tbl_PredefinedCopyDescriptionId",
                        column: x => x.PredefinedCopyDescriptionId,
                        principalTable: "PredefinedCopyDescription_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_TaskSchedule_Tbl_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskScheduleAssignment_TaskSchedule_Tbl_TaskScheduleId",
                        column: x => x.TaskScheduleId,
                        principalTable: "TaskSchedule_Tbl",
                        principalColumn: "Id");
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
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
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
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityHistory_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityCRM_Tbl_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CRMAttachment_Tbl_CRMInteraction_Tbl_CRMInteractionId",
                        column: x => x.CRMInteractionId,
                        principalTable: "CRMInteraction_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CRMParticipant_Tbl_StakeholderContact_Tbl_StakeholderContactId",
                        column: x => x.StakeholderContactId,
                        principalTable: "StakeholderContact_Tbl",
                        principalColumn: "Id");
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CRMTeam_Tbl_Team_Tbl_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        onDelete: ReferentialAction.Cascade);
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
                    AssignedUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignerUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignmentType = table.Column<byte>(type: "tinyint", nullable: false),
                    CopyDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PredefinedCopyDescriptionId = table.Column<int>(type: "int", nullable: true),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    IsMyDay = table.Column<bool>(type: "bit", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskAssignment_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploaderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAttachment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAttachment_Tbl_AspNetUsers_UploaderUserId",
                        column: x => x.UploaderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskAttachment_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskComment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true)
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
                name: "TaskOperation_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperationOrder = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsViewed = table.Column<bool>(type: "bit", nullable: false),
                    ViewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskCommentAttachment_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploaderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCommentAttachment_Tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCommentAttachment_Tbl_AspNetUsers_UploaderUserId",
                        column: x => x.UploaderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskCommentAttachment_Tbl_TaskComment_Tbl_CommentId",
                        column: x => x.CommentId,
                        principalTable: "TaskComment_Tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskCommentMention_Tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    MentionedUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MentionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
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
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NotificationType = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskNotification_Tbl_TaskOperation_Tbl_OperationId",
                        column: x => x.OperationId,
                        principalTable: "TaskOperation_Tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskNotification_Tbl_Tasks_Tbl_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks_Tbl",
                        principalColumn: "Id");
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
                name: "IX_BranchUser_Tbl_AssignedByUserId",
                table: "BranchUser_Tbl",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchUser_Tbl_BranchId",
                table: "BranchUser_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchUser_Tbl_UserId",
                table: "BranchUser_Tbl",
                column: "UserId");

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
                name: "IX_RolePatternDetails_Tbl_RoleID",
                table: "RolePatternDetails_Tbl",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_RolePatternDetails_Tbl_RolePatternID",
                table: "RolePatternDetails_Tbl",
                column: "RolePatternID");

            migrationBuilder.CreateIndex(
                name: "IX_Stakeholder_Tbl_BranchId",
                table: "Stakeholder_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Stakeholder_Tbl_CreatorUserId",
                table: "Stakeholder_Tbl",
                column: "CreatorUserId");

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
                name: "IX_StakeholderContact_Tbl_StakeholderId",
                table: "StakeholderContact_Tbl",
                column: "StakeholderId");

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
                name: "IX_TaskAssignment_Tbl_TaskId",
                table: "TaskAssignment_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAttachment_Tbl_TaskId",
                table: "TaskAttachment_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAttachment_Tbl_UploaderUserId",
                table: "TaskAttachment_Tbl",
                column: "UploaderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategory_Tbl_ParentCategoryId",
                table: "TaskCategory_Tbl",
                column: "ParentCategoryId");

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
                name: "IX_TaskCommentAttachment_Tbl_CommentId",
                table: "TaskCommentAttachment_Tbl",
                column: "CommentId");

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
                name: "IX_TaskNotification_Tbl_TaskId",
                table: "TaskNotification_Tbl",
                column: "TaskId");

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
                name: "IX_Tasks_Tbl_BranchId",
                table: "Tasks_Tbl",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_BranchId1",
                table: "Tasks_Tbl",
                column: "BranchId1");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_ContractId",
                table: "Tasks_Tbl",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Tbl_CreatorUserId",
                table: "Tasks_Tbl",
                column: "CreatorUserId");

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
                name: "IX_TaskScheduleAssignment_CreatorUserId",
                table: "TaskScheduleAssignment",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_PredefinedCopyDescriptionId",
                table: "TaskScheduleAssignment",
                column: "PredefinedCopyDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_ScheduleId",
                table: "TaskScheduleAssignment",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_TaskScheduleId",
                table: "TaskScheduleAssignment",
                column: "TaskScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskScheduleAssignment_UserId",
                table: "TaskScheduleAssignment",
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
                name: "IX_TaskTemplate_Tbl_CategoryId",
                table: "TaskTemplate_Tbl",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplate_Tbl_CreatorUserId",
                table: "TaskTemplate_Tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplateOperation_Tbl_TemplateId",
                table: "TaskTemplateOperation_Tbl",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_AddedByUserId",
                table: "TaskViewer_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_TaskId",
                table: "TaskViewer_Tbl",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskViewer_Tbl_UserId",
                table: "TaskViewer_Tbl",
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
                name: "IX_TeamMember_Tbl_AddedByUserId",
                table: "TeamMember_Tbl",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_Tbl_TeamId",
                table: "TeamMember_Tbl",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_Tbl_UserId",
                table: "TeamMember_Tbl",
                column: "UserId");
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
                name: "BranchUser_Tbl");

            migrationBuilder.DropTable(
                name: "CRMAttachment_Tbl");

            migrationBuilder.DropTable(
                name: "CRMComment_Tbl");

            migrationBuilder.DropTable(
                name: "CRMParticipant_Tbl");

            migrationBuilder.DropTable(
                name: "CRMTeam_Tbl");

            migrationBuilder.DropTable(
                name: "RolePatternDetails_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderBranch_Tbl");

            migrationBuilder.DropTable(
                name: "TaskAssignment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskAttachment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskCommentAttachment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskCommentMention_Tbl");

            migrationBuilder.DropTable(
                name: "TaskNotification_Tbl");

            migrationBuilder.DropTable(
                name: "TaskScheduleAssignment");

            migrationBuilder.DropTable(
                name: "TaskScheduleExecution");

            migrationBuilder.DropTable(
                name: "TaskTemplateOperation_Tbl");

            migrationBuilder.DropTable(
                name: "TaskViewer_Tbl");

            migrationBuilder.DropTable(
                name: "TeamMember_Tbl");

            migrationBuilder.DropTable(
                name: "ActivityBase_Tbl");

            migrationBuilder.DropTable(
                name: "CRMInteraction_Tbl");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "RolePattern_Tbl");

            migrationBuilder.DropTable(
                name: "TaskComment_Tbl");

            migrationBuilder.DropTable(
                name: "TaskOperation_Tbl");

            migrationBuilder.DropTable(
                name: "PredefinedCopyDescription_Tbl");

            migrationBuilder.DropTable(
                name: "StakeholderContact_Tbl");

            migrationBuilder.DropTable(
                name: "Tasks_Tbl");

            migrationBuilder.DropTable(
                name: "Contract_Tbl");

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
