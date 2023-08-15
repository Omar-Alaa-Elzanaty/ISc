using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISC.EF.Migrations
{
    /// <inheritdoc />
    public partial class addNavigationHeadOfTraining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeadOfTraining_Accounts_UserId",
                table: "HeadOfTraining");

            migrationBuilder.DropForeignKey(
                name: "FK_HeadOfTraining_Camps_CampId",
                table: "HeadOfTraining");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HeadOfTraining",
                table: "HeadOfTraining");

            migrationBuilder.RenameTable(
                name: "HeadOfTraining",
                newName: "HeadsOfTraining");

            migrationBuilder.RenameIndex(
                name: "IX_HeadOfTraining_UserId",
                table: "HeadsOfTraining",
                newName: "IX_HeadsOfTraining_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_HeadOfTraining_CampId",
                table: "HeadsOfTraining",
                newName: "IX_HeadsOfTraining_CampId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HeadsOfTraining",
                table: "HeadsOfTraining",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HeadsOfTraining_Accounts_UserId",
                table: "HeadsOfTraining",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HeadsOfTraining_Camps_CampId",
                table: "HeadsOfTraining",
                column: "CampId",
                principalTable: "Camps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeadsOfTraining_Accounts_UserId",
                table: "HeadsOfTraining");

            migrationBuilder.DropForeignKey(
                name: "FK_HeadsOfTraining_Camps_CampId",
                table: "HeadsOfTraining");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HeadsOfTraining",
                table: "HeadsOfTraining");

            migrationBuilder.RenameTable(
                name: "HeadsOfTraining",
                newName: "HeadOfTraining");

            migrationBuilder.RenameIndex(
                name: "IX_HeadsOfTraining_UserId",
                table: "HeadOfTraining",
                newName: "IX_HeadOfTraining_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_HeadsOfTraining_CampId",
                table: "HeadOfTraining",
                newName: "IX_HeadOfTraining_CampId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HeadOfTraining",
                table: "HeadOfTraining",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HeadOfTraining_Accounts_UserId",
                table: "HeadOfTraining",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HeadOfTraining_Camps_CampId",
                table: "HeadOfTraining",
                column: "CampId",
                principalTable: "Camps",
                principalColumn: "Id");
        }
    }
}
