using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkJobPostingBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookingId",
                table: "JobPostings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceJobPostingId",
                table: "Bookings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SourceJobPostingId",
                table: "Bookings",
                column: "SourceJobPostingId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_JobPostings_SourceJobPostingId",
                table: "Bookings",
                column: "SourceJobPostingId",
                principalTable: "JobPostings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_JobPostings_SourceJobPostingId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SourceJobPostingId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SourceJobPostingId",
                table: "Bookings");
        }
    }
}
