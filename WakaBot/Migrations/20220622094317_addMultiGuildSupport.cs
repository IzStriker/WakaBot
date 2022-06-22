using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WakaBot.Migrations
{
    public partial class addMultiGuildSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Users");
        }
    }
}
