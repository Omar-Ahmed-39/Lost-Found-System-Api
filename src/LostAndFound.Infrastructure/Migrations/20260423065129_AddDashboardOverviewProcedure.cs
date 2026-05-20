using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostAndFound.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardOverviewProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
                CREATE OR ALTER PROCEDURE sp_GetOverviewStats
                    @StartOfThisWeek DATETIME2,
                    @StartOfLastWeek DATETIME2
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @TotalActiveReports INT, @ActiveReportsThisWeek INT,
                            @TotalPendingClaims INT, @PendingClaimsLastWeek INT,
                            @TotalResolvedReports INT, @TotalReports INT,
                            @TotalUsersCount INT, @NewUsersThisWeek INT;

                    -- Reports Metrics
                    SELECT 
                        @TotalActiveReports = SUM(CASE WHEN StatusType IN ('Open', 'UnderReview') THEN 1 ELSE 0 END),
                        @ActiveReportsThisWeek = SUM(CASE WHEN StatusType IN ('Open', 'UnderReview') AND CreatedAt >= @StartOfThisWeek THEN 1 ELSE 0 END),
                        @TotalResolvedReports = SUM(CASE WHEN StatusType IN ('Returned', 'Closed') THEN 1 ELSE 0 END),
                        @TotalReports = COUNT(*)
                    FROM ItemReports;

                    -- Claims Metrics
                    SELECT 
                        @TotalPendingClaims = SUM(CASE WHEN ApprovalStatus = 'Pending' THEN 1 ELSE 0 END),
                        @PendingClaimsLastWeek = SUM(CASE WHEN ApprovalStatus = 'Pending' AND CreatedAt >= @StartOfLastWeek AND CreatedAt < @StartOfThisWeek THEN 1 ELSE 0 END)
                    FROM Claims;

                    -- Users Metrics
                    SELECT 
                        @TotalUsersCount = COUNT(*),
                        @NewUsersThisWeek = SUM(CASE WHEN Created >= @StartOfThisWeek THEN 1 ELSE 0 END)
                    FROM Users;

                    -- Resolve NULLs
                    SET @TotalActiveReports = ISNULL(@TotalActiveReports, 0);
                    SET @ActiveReportsThisWeek = ISNULL(@ActiveReportsThisWeek, 0);
                    SET @TotalPendingClaims = ISNULL(@TotalPendingClaims, 0);
                    SET @PendingClaimsLastWeek = ISNULL(@PendingClaimsLastWeek, 0);
                    SET @TotalResolvedReports = ISNULL(@TotalResolvedReports, 0);
                    SET @TotalReports = ISNULL(@TotalReports, 0);
                    SET @TotalUsersCount = ISNULL(@TotalUsersCount, 0);
                    SET @NewUsersThisWeek = ISNULL(@NewUsersThisWeek, 0);

                    DECLARE @RecoveryRate FLOAT = 0.0;
                    IF @TotalReports > 0
                    BEGIN
                        SET @RecoveryRate = ROUND(CAST(@TotalResolvedReports AS FLOAT) / @TotalReports * 100.0, 1);
                    END

                    SELECT 
                        @TotalActiveReports AS ActiveReports,
                        @ActiveReportsThisWeek AS ActiveReportsThisWeek,
                        @TotalPendingClaims AS TotalPendingClaims,
                        @PendingClaimsLastWeek AS PendingClaimsLastWeek,
                        @RecoveryRate AS RecoveryRate,
                        @TotalUsersCount AS TotalUsersCount,
                        @NewUsersThisWeek AS NewUsersThisWeek;
                END";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetOverviewStats");
        }
    }
}
