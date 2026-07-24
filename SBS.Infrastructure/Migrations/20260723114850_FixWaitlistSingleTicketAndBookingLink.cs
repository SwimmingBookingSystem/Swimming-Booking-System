using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixWaitlistSingleTicketAndBookingLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "WaitlistEntries",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "WaitlistEntries",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE WaitlistEntries SET Quantity = 1 WHERE Status = 'Waiting';");

            migrationBuilder.Sql(
                @"UPDATE w
                  SET w.BookingId = matched.BookingId
                  FROM WaitlistEntries AS w
                  CROSS APPLY
                  (
                      SELECT TOP (1) b.BookingId
                      FROM Bookings AS b
                      WHERE b.UserId = w.UserId
                        AND b.PoolSlotId = w.PoolSlotId
                        AND b.Status IN ('PendingPayment', 'Paid', 'CheckIn', 'Completed')
                      ORDER BY b.CreatedAt DESC, b.BookingId DESC
                  ) AS matched
                  WHERE w.Status = 'Offered' AND w.BookingId IS NULL;");

            migrationBuilder.Sql(
                @"UPDATE w
                  SET w.Status = 'Purchased'
                  FROM WaitlistEntries AS w
                  INNER JOIN Bookings AS b ON b.BookingId = w.BookingId
                  WHERE w.Status = 'Offered' AND b.Status IN ('Paid', 'CheckIn', 'Completed');");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_BookingId",

                table: "WaitlistEntries",
                column: "BookingId",
                unique: true,
                filter: "[BookingId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_WaitlistEntries_Bookings_BookingId",
                table: "WaitlistEntries",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistEntries_Bookings_BookingId",
                table: "WaitlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WaitlistEntries_BookingId",
                table: "WaitlistEntries");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "WaitlistEntries");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "WaitlistEntries",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);
        }
    }
}
