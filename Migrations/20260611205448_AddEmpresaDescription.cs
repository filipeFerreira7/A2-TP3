using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace a2_tp3_job_connect.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Empresas",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Empresas");
        }
    }
}
