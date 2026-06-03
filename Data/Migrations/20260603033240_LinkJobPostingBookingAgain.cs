using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkJobPostingBookingAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Fill Bookings.SourceJobPostingId where missing
            migrationBuilder.Sql(@"
        UPDATE Bookings
        SET SourceJobPostingId = (
            SELECT jp.Id
            FROM JobPostings jp
            WHERE jp.CustomerId = Bookings.CustomerId
              AND jp.AssignedWorkerId = Bookings.WorkerId
              AND jp.Title = Bookings.Title
              AND jp.StartDate = Bookings.StartDate
              AND jp.EndDate = Bookings.EndDate
            LIMIT 1
        )
        WHERE SourceJobPostingId IS NULL
          AND WorkerId IS NOT NULL;
    ");

            // 2) Mirror into JobPostings.BookingId where missing
            migrationBuilder.Sql(@"
        UPDATE JobPostings
        SET BookingId = (
            SELECT b.Id
            FROM Bookings b
            WHERE b.SourceJobPostingId = JobPostings.Id
            LIMIT 1
        )
        WHERE BookingId IS NULL;
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // If we rollback this migration, undo the backfill by setting the columns back to null
            // (Only doing this for records that were presumably linked by this script)
            migrationBuilder.Sql("UPDATE Bookings SET SourceJobPostingId = NULL WHERE SourceJobPostingId IS NOT NULL;");
            migrationBuilder.Sql("UPDATE JobPostings SET BookingId = NULL WHERE BookingId IS NOT NULL;");
        }
    }
}
