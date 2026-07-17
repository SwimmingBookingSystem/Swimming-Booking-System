using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePoolTicketPriceOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "PoolTicketTypes",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "PoolTicketPriceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoolTicketTypeId = table.Column<int>(type: "int", nullable: false),
                    OldCustomPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NewCustomPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolTicketPriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoolTicketPriceHistories_PoolTicketTypes_PoolTicketTypeId",
                        column: x => x.PoolTicketTypeId,
                        principalTable: "PoolTicketTypes",
                        principalColumn: "PoolTicketTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoolTicketPriceHistories_PoolTicketTypeId",
                table: "PoolTicketPriceHistories",
                column: "PoolTicketTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoolTicketPriceHistories");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "PoolTicketTypes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }
    }
}
