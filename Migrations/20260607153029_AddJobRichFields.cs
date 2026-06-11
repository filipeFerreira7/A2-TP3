using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace a2_tp3_job_connect.Migrations
{
    /// <inheritdoc />
    public partial class AddJobRichFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyDescription",
                table: "Vagas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "Vagas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Responsibilities",
                table: "Vagas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Schedule",
                table: "Vagas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyDescription",
                table: "Vagas");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "Vagas");

            migrationBuilder.DropColumn(
                name: "Responsibilities",
                table: "Vagas");

            migrationBuilder.DropColumn(
                name: "Schedule",
                table: "Vagas");
        }
    }
}
