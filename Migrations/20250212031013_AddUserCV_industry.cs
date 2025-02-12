using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCV_industry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobIndustry",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobIndustry",
                table: "AspNetUsers");
        }
    }
}
