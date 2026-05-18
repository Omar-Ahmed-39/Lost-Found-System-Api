using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostAndFound.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addmatchuniqueindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Matches_LostId",
                table: "Matches");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LostId_FoundId",
                table: "Matches",
                columns: new[] { "LostId", "FoundId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Matches_LostId_FoundId",
                table: "Matches");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LostId",
                table: "Matches",
                column: "LostId");
        }
    }
}
