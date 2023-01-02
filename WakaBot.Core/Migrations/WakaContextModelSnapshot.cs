﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WakaBot.Core.Data;

#nullable disable

namespace WakaBot.Core.Migrations
{
    [DbContext(typeof(WakaContext))]
    partial class WakaContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("DiscordGuildDiscordUser", b =>
                {
                    b.Property<ulong>("GuildsId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UsersId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("GuildsId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("DiscordGuildDiscordUser");
                });

            modelBuilder.Entity("WakaBot.Core.Models.DiscordGuild", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("DiscordGuilds");
                });

            modelBuilder.Entity("WakaBot.Core.Models.DiscordUser", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("WakaUserId")
                        .HasColumnType("varchar(95)");

                    b.HasKey("Id");

                    b.HasIndex("WakaUserId")
                        .IsUnique();

                    b.ToTable("DiscordUsers");
                });

            modelBuilder.Entity("WakaBot.Core.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("DiscordId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("WakaName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("WakaBot.Core.Models.WakaUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(95)");

                    b.Property<string>("AccessToken")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("longtext");

                    b.Property<string>("Scope")
                        .HasColumnType("longtext");

                    b.Property<string>("State")
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("usingOAuth")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.ToTable("WakaUsers");
                });

            modelBuilder.Entity("DiscordGuildDiscordUser", b =>
                {
                    b.HasOne("WakaBot.Core.Models.DiscordGuild", null)
                        .WithMany()
                        .HasForeignKey("GuildsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WakaBot.Core.Models.DiscordUser", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WakaBot.Core.Models.DiscordUser", b =>
                {
                    b.HasOne("WakaBot.Core.Models.WakaUser", "WakaUser")
                        .WithMany()
                        .HasForeignKey("WakaUserId");

                    b.Navigation("WakaUser");
                });
#pragma warning restore 612, 618
        }
    }
}
