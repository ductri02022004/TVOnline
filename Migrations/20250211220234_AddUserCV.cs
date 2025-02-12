using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CvFileUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvFileUrl",
                table: "AspNetUsers");
        }
    }
}
