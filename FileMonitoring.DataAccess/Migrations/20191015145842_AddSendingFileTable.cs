using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileMonitoring.DataAccess.Migrations
{
    public partial class AddSendingFileTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SendingFiles",
                schema: "file_monitoring",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendingFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SendingFiles_Id",
                schema: "file_monitoring",
                table: "SendingFiles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SendingFiles_Status",
                schema: "file_monitoring",
                table: "SendingFiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SendingFiles_Path_FileName_CreateDate",
                schema: "file_monitoring",
                table: "SendingFiles",
                columns: new[] { "Path", "FileName", "CreateDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SendingFiles",
                schema: "file_monitoring");
        }
    }
}
