using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISC.EF.Migrations
{
    /// <inheritdoc />
    public partial class addRateandcampName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rate",
                table: "SessionsFeedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CampName",
                table: "NewRegitserations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                table: "SessionsFeedbacks");

            migrationBuilder.DropColumn(
                name: "CampName",
                table: "NewRegitserations");
        }
    }
}
