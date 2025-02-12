using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCV_Age : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Age",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "AspNetUsers");
        }
    }
}
