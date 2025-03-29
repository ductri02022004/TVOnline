using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CVStatus",
                table: "UserCVs",
                newName: "CvStatus");

            migrationBuilder.RenameColumn(
                name: "CVFileUrl",
                table: "UserCVs",
                newName: "CvFileUrl");

            migrationBuilder.RenameColumn(
                name: "CvID",
                table: "UserCVs",
                newName: "CvId");

            migrationBuilder.RenameColumn(
                name: "TemplateFileURL",
                table: "Templates",
                newName: "TemplateFileUrl");

            migrationBuilder.RenameColumn(
                name: "LogoURL",
                table: "Employers",
                newName: "LogoUrl");

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "CvStatus",
                table: "UserCVs",
                newName: "CVStatus");

            migrationBuilder.RenameColumn(
                name: "CvFileUrl",
                table: "UserCVs",
                newName: "CVFileUrl");

            migrationBuilder.RenameColumn(
                name: "CvId",
                table: "UserCVs",
                newName: "CvID");

            migrationBuilder.RenameColumn(
                name: "TemplateFileUrl",
                table: "Templates",
                newName: "TemplateFileURL");

            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "Employers",
                newName: "LogoURL");
        }
    }
}
