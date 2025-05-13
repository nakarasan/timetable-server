using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Time_Table_Generator.Migrations
{
    /// <inheritdoc />
    public partial class adddaytotimetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Day",
                table: "TimeTables",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "TimeTables");
        }
    }
}
