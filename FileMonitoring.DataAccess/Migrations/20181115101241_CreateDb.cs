using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileMonitoring.DataAccess.Migrations
{
    public partial class CreateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "file_monitoring");

            migrationBuilder.CreateTable(
                name: "SendingTasks",
                schema: "file_monitoring",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    Recipient = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ProcessDate = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Uid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendingTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SendingTasks_Id",
                schema: "file_monitoring",
                table: "SendingTasks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SendingTasks_Status",
                schema: "file_monitoring",
                table: "SendingTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SendingTasks_Path_Name_CreateDate",
                schema: "file_monitoring",
                table: "SendingTasks",
                columns: new[] { "Path", "Name", "CreateDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SendingTasks",
                schema: "file_monitoring");
        }
    }
}
