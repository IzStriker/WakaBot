using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WakaBot.Core.Migrations
{
    public partial class modelRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordGuilds",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGuilds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WakaUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usingOAuth = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefreshToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Scope = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WakaUsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DiscordUsers",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    WakaUserId = table.Column<string>(type: "varchar(95)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordUsers_WakaUsers_WakaUserId",
                        column: x => x.WakaUserId,
                        principalTable: "WakaUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DiscordGuildDiscordUser",
                columns: table => new
                {
                    GuildsId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UsersId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGuildDiscordUser", x => new { x.GuildsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_DiscordGuildDiscordUser_DiscordGuilds_GuildsId",
                        column: x => x.GuildsId,
                        principalTable: "DiscordGuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordGuildDiscordUser_DiscordUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "DiscordUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordGuildDiscordUser_UsersId",
                table: "DiscordGuildDiscordUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_WakaUserId",
                table: "DiscordUsers",
                column: "WakaUserId",
                unique: true);

            // move data from User Table into DiscordUser, DiscordGuild, and DiscordGuildDiscordUser tables
            migrationBuilder.Sql("INSERT INTO DiscordUsers (Id) SELECT DISTINCT DiscordId FROM Users;");
            migrationBuilder.Sql("INSERT INTO DiscordGuilds (Id) SELECT DISTINCT GuildId FROM Users;");
            migrationBuilder.Sql("INSERT INTO DiscordGuildDiscordUser (GuildsId, UsersId) SELECT GuildId, DiscordId FROM Users;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordGuildDiscordUser");

            migrationBuilder.DropTable(
                name: "DiscordGuilds");

            migrationBuilder.DropTable(
                name: "DiscordUsers");

            migrationBuilder.DropTable(
                name: "WakaUsers");
        }
    }
}
