using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePoolCapacity_AddArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Area",
                table: "Pools",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "StandardCapacity",
                table: "Pools",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "StandardCapacity",
                table: "Pools");
        }
    }
}
