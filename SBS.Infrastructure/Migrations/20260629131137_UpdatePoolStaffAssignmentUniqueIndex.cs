using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePoolStaffAssignmentUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PoolStaffAssignments_PoolId_StaffId",
                table: "PoolStaffAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_PoolStaffAssignments_PoolId",
                table: "PoolStaffAssignments",
                column: "PoolId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PoolStaffAssignments_PoolId",
                table: "PoolStaffAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_PoolStaffAssignments_PoolId_StaffId",
                table: "PoolStaffAssignments",
                columns: new[] { "PoolId", "StaffId" },
                unique: true);
        }
    }
}
