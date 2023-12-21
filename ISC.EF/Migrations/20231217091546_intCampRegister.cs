using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISC.EF.Migrations
{
    /// <inheritdoc />
    public partial class intCampRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampName",
                table: "NewRegistration");

            migrationBuilder.DropColumn(
                name: "ProfilePictrue",
                table: "NewRegistration");

            migrationBuilder.AddColumn<int>(
                name: "CampId",
                table: "NewRegistration",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "NewRegistration",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampId",
                table: "NewRegistration");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "NewRegistration");

            migrationBuilder.AddColumn<string>(
                name: "CampName",
                table: "NewRegistration",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePictrue",
                table: "NewRegistration",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
