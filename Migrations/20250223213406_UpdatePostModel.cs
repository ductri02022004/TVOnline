using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employers_AspNetUsers_UserId",
                table: "Employers");

            migrationBuilder.DropIndex(
                name: "IX_Employers_UserId",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Posts");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Benefits",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Posts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Experience",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobType",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Posts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Employers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CityId",
                table: "Posts",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employers_AspNetUsers_EmployerId",
                table: "Employers",
                column: "EmployerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Cities_CityId",
                table: "Posts",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "CityId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employers_AspNetUsers_EmployerId",
                table: "Employers");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Cities_CityId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CityId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Benefits",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Posts");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Employers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employers_UserId",
                table: "Employers",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Employers_AspNetUsers_UserId",
                table: "Employers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
