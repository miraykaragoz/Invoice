using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice.Migrations
{
    /// <inheritdoc />
    public partial class Date : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Invoices",
                newName: "ProjectDescription");

            migrationBuilder.RenameColumn(
                name: "CreatedTime",
                table: "Invoices",
                newName: "InvoiceDate");

            migrationBuilder.AddColumn<int>(
                name: "PaymentTerm",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentTerm",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "ProjectDescription",
                table: "Invoices",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "InvoiceDate",
                table: "Invoices",
                newName: "CreatedTime");
        }
    }
}
