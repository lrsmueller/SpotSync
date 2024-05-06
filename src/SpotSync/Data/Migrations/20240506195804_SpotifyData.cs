using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotSync.Migrations
{
    /// <inheritdoc />
    public partial class SpotifyData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRefresh",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "RefreshHour",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SelectedPlaylist",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRefresh",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshHour",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SelectedPlaylist",
                table: "AspNetUsers");
        }
    }
}
