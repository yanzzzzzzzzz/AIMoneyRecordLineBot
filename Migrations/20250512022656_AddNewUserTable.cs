using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIMoneyRecordLineBot.Migrations
{
    /// <inheritdoc />
    public partial class AddNewUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineUserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LineDisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
