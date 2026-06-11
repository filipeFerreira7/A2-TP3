using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace a2_tp3_job_connect.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationFormFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityPreference",
                table: "Candidaturas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienceNotes",
                table: "Candidaturas",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryExpectation",
                table: "Candidaturas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityPreference",
                table: "Candidaturas");

            migrationBuilder.DropColumn(
                name: "ExperienceNotes",
                table: "Candidaturas");

            migrationBuilder.DropColumn(
                name: "SalaryExpectation",
                table: "Candidaturas");
        }
    }
}
