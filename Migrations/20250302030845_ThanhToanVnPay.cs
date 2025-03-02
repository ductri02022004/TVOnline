using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class ThanhToanVnPay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Employers_EmployerId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewInvitations_Employers_EmployerId",
                table: "InterviewInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Employers_EmployerId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Payments",
                newName: "UsersId");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Payments",
                newName: "OrderDescription");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "Payments",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                newName: "IX_Payments_UsersId");

            migrationBuilder.AlterColumn<string>(
                name: "EmployerId",
                table: "Posts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderType",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "EmployerId",
                table: "InterviewInvitations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "EmployerId",
                table: "Feedbacks",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Employers_EmployerId",
                table: "Feedbacks",
                column: "EmployerId",
                principalTable: "Employers",
                principalColumn: "EmployerId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewInvitations_Employers_EmployerId",
                table: "InterviewInvitations",
                column: "EmployerId",
                principalTable: "Employers",
                principalColumn: "EmployerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UsersId",
                table: "Payments",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Employers_EmployerId",
                table: "Posts",
                column: "EmployerId",
                principalTable: "Employers",
                principalColumn: "EmployerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Employers_EmployerId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewInvitations_Employers_EmployerId",
                table: "InterviewInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UsersId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Employers_EmployerId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OrderType",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "UsersId",
                table: "Payments",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "OrderDescription",
                table: "Payments",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Payments",
                newName: "PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_UsersId",
                table: "Payments",
                newName: "IX_Payments_UserId");

            migrationBuilder.AlterColumn<int>(
                name: "EmployerId",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmployerId",
                table: "InterviewInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmployerId",
                table: "Feedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Employers_EmployerId",
                table: "Feedbacks",
                column: "EmployerId",
                principalTable: "Employers",
                principalColumn: "EmployerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewInvitations_Employers_EmployerId",
                table: "InterviewInvitations",
                column: "EmployerId",
                principalTable: "Employers",
                principalColumn: "EmployerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Employers_EmployerId",
                table: "Posts",
                column: "EmployerId",
                principalTable: "Employers",
                principalColumn: "EmployerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
