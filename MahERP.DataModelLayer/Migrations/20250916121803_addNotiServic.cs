using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class addNotiServic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoreNotificationDelivery_Tbl");

            migrationBuilder.DropTable(
                name: "CoreNotificationDetail_Tbl");

            migrationBuilder.DropTable(
                name: "CoreNotificationSetting_Tbl");

            migrationBuilder.DropTable(
                name: "CoreNotification_Tbl");
        }
    }
}
