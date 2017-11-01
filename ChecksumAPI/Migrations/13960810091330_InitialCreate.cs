using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ChecksumAPI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileChecksums",
                columns: table => new
                {
                    FileUrl = table.Column<string>(type: "TEXT", nullable: false),
                    OffsetPercent = table.Column<byte>(type: "INTEGER", nullable: false),
                    Algorithm = table.Column<string>(type: "TEXT", nullable: false),
                    Checksum = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileChecksums", x => new { x.FileUrl, x.OffsetPercent });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileChecksums");
        }
    }
}
