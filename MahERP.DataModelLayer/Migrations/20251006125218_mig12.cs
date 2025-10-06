using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahERP.DataModelLayer.Migrations
{
    /// <inheritdoc />
    public partial class mig12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "Branch_Tbl",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "Description", "Name" },
                values: new object[] { new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "شعبه برند رسنا", "شعبه رسنا" });

            migrationBuilder.UpdateData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DisplayOrder", "Title" },
                values: new object[] { "تسک‌های مربوط به فروش", (byte)4, "فروش" });

            migrationBuilder.UpdateData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "DisplayOrder", "Title" },
                values: new object[] { "تسک‌های مربوط به خدمات مشتریان غیر حضوری", (byte)5, "خدمات حضوری" });

            migrationBuilder.UpdateData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Title" },
                values: new object[] { "تسک‌های مربوط به خدمات مشتریان حضوری", "خدمات  غیر حضوری" });

            migrationBuilder.UpdateData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "DisplayOrder", "Title" },
                values: new object[] { "تسک‌های فوری و اضطراری", (byte)10, "فوری" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Branch_Tbl",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "Description", "Name" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "شعبه اصلی سازمان", "دفتر مرکزی" });

            migrationBuilder.UpdateData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DisplayOrder", "Title" },
                values: new object[] { "تسک‌های فنی و تخصصی", (byte)3, "فنی" });

            migrationBuilder.UpdateData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "DisplayOrder", "Title" },
                values: new object[] { "تسک‌های مربوط به فروش", (byte)4, "فروش" });

            migrationBuilder.UpdateData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Title" },
                values: new object[] { "تسک‌های مربوط به خدمات مشتریان", "خدمات" });

            migrationBuilder.UpdateData(
                table: "TaskCategory_Tbl",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "DisplayOrder", "Title" },
                values: new object[] { "تسک‌های پروژه‌ای", (byte)9, "پروژه" });

            migrationBuilder.InsertData(
                table: "TaskCategory_Tbl",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "ParentCategoryId", "TaskCategoryId", "Title" },
                values: new object[] { 10, "تسک‌های فوری و اضطراری", (byte)10, true, null, null, "فوری" });
        }
    }
}
