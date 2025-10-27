using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiparisApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleColumnToAllowedEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AllowedEmails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AllowedEmails");

        }
    }
}
