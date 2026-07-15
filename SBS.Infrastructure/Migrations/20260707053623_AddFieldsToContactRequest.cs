using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToContactRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "ContactRequests",
                newName: "Message");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ContactRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "ContactRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "ContactRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "ContactRequests");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "ContactRequests");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "ContactRequests");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "ContactRequests",
                newName: "Reason");
        }
    }
}
