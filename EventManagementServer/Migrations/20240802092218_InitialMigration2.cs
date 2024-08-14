using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagementServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "TeamMemberNumber",
                table: "Registrations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Registrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamMemberNumber",
                table: "Registrations",
                type: "text",
                nullable: true);
        }
    }
}
