using ISC.Core.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISC.EF.Migrations
{
    /// <inheritdoc />
    public partial class dataSedding : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.InsertData(
					table: "Roles",
					columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
					values: new object[] { Guid.NewGuid().ToString(),Role.LEADER,
					Role.LEADER.ToUpper(),Guid.NewGuid().ToString() }
				);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("Delete * from [Roles]");
		}
	}
}
