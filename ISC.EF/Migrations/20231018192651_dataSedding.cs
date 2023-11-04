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
			migrationBuilder.InsertData(
					table: "Roles",
					columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
					values: new object[] { Guid.NewGuid().ToString(),Role.TRAINEE,
					Role.TRAINEE.ToUpper(),Guid.NewGuid().ToString() }
				);
			migrationBuilder.InsertData(
					table: "Roles",
					columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
					values: new object[] { Guid.NewGuid().ToString(),Role.MENTOR,
					Role.MENTOR.ToUpper(),Guid.NewGuid().ToString() }
				);
			migrationBuilder.InsertData(
					table: "Roles",
					columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
					values: new object[] { Guid.NewGuid().ToString(),Role.HOC,
					Role.HOC.ToUpper(),Guid.NewGuid().ToString() }
				);
			migrationBuilder.InsertData(
					table: "Roles",
					columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
					values: new object[] { Guid.NewGuid().ToString(),Role.INSTRUCTOR,
					Role.INSTRUCTOR.ToUpper(),Guid.NewGuid().ToString() }
				);
			migrationBuilder.InsertData(
					table: "Roles",
					columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
					values: new object[] { Guid.NewGuid().ToString(),"Admin",
					"ADMIN",Guid.NewGuid().ToString() }
				);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("Delete from [Roles]");
		}
	}
}
