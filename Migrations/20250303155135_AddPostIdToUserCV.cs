using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddPostIdToUserCV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostId",
                table: "UserCVs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCVs_PostId",
                table: "UserCVs",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCVs_Posts_PostId",
                table: "UserCVs",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCVs_Posts_PostId",
                table: "UserCVs");

            migrationBuilder.DropIndex(
                name: "IX_UserCVs_PostId",
                table: "UserCVs");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "UserCVs");
        }
    }
}
