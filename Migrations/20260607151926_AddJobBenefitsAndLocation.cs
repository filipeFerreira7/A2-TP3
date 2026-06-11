using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace a2_tp3_job_connect.Migrations
{
    /// <inheritdoc />
    public partial class AddJobBenefitsAndLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Benefits",
                table: "Vagas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Vagas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Benefits",
                table: "Vagas");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Vagas");
        }
    }
}
