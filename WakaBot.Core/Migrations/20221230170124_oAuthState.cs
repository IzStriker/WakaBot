using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WakaBot.Core.Migrations
{
    public partial class oAuthState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "WakaUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "WakaUsers");
        }
    }
}
