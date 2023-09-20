using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISC.EF.Migrations
{
    /// <inheritdoc />
    public partial class editArchiveandNewRegisterion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewRegitserations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TraineesArchives",
                table: "TraineesArchives");

            migrationBuilder.AlterColumn<string>(
                name: "CampName",
                table: "TraineesArchives",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TraineesArchives",
                table: "TraineesArchives",
                columns: new[] { "NationalID", "CampName" });

            migrationBuilder.CreateTable(
                name: "NewRegistration",
                columns: table => new
                {
                    NationalID = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    College = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeForceHandle = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FacebookLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VjudgeHandle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePictrue = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CampName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasLaptop = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewRegistration", x => x.NationalID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewRegistration_CodeForceHandle",
                table: "NewRegistration",
                column: "CodeForceHandle",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewRegistration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TraineesArchives",
                table: "TraineesArchives");

            migrationBuilder.AlterColumn<string>(
                name: "CampName",
                table: "TraineesArchives",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TraineesArchives",
                table: "TraineesArchives",
                column: "NationalID");

            migrationBuilder.CreateTable(
                name: "NewRegitserations",
                columns: table => new
                {
                    NationalID = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CampName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeForceHandle = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    College = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacebookLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePictrue = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    VjudgeHandle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewRegitserations", x => x.NationalID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewRegitserations_CodeForceHandle",
                table: "NewRegitserations",
                column: "CodeForceHandle",
                unique: true);
        }
    }
}
