using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Service.Infrastructure.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("ebaec3e3-ea51-42ae-a997-90ad524a8323"));

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Password", "Role", "Username" },
                values: new object[] { new Guid("01b72ebe-eb72-41ce-893c-ef926ca62da3"), "Gdeme19", "Administrator", "Gaga" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("01b72ebe-eb72-41ce-893c-ef926ca62da3"));

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Password", "Username" },
                values: new object[] { new Guid("ebaec3e3-ea51-42ae-a997-90ad524a8323"), "Gdeme19", "Gaga" });
        }
    }
}
