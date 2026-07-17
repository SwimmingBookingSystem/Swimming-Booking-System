using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketPriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketPriceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketTypeId = table.Column<int>(type: "int", nullable: false),
                    OldBasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewBasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OldDiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewDiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketPriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketPriceHistories_TicketTypes_TicketTypeId",
                        column: x => x.TicketTypeId,
                        principalTable: "TicketTypes",
                        principalColumn: "TicketTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketPriceHistories_TicketTypeId",
                table: "TicketPriceHistories",
                column: "TicketTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketPriceHistories");
        }
    }
}
