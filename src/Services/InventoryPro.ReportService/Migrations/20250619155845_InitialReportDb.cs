using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPro.ReportService.Migrations
{
    /// <inheritdoc />
    public partial class InitialReportDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Format = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordCount = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportRecords_CreatedAt",
                table: "ReportRecords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRecords_ReportType",
                table: "ReportRecords",
                column: "ReportType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportRecords");
        }
    }
}
