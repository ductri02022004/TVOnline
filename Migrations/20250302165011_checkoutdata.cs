using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class checkoutdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Employers_EmployerId",
                table: "Feedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentInformationModel",
                table: "PaymentInformationModel");

            migrationBuilder.AlterColumn<string>(
                name: "OrderType",
                table: "PaymentInformationModel",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "PaymentInformationModel",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentInformationModel",
                table: "PaymentInformationModel",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Employers_EmployerId",
                table: "Feedbacks",
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentInformationModel",
                table: "PaymentInformationModel");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "PaymentInformationModel");

            migrationBuilder.AlterColumn<string>(
                name: "OrderType",
                table: "PaymentInformationModel",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentInformationModel",
                table: "PaymentInformationModel",
                column: "OrderType");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Employers_EmployerId",
                table: "Feedbacks",
                column: "EmployerId",
                principalTable: "Employers",
                principalColumn: "EmployerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
