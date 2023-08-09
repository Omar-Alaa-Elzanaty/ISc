using ISC.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.ModelsConfigurations
{
	internal class SheetConfigurations : IEntityTypeConfiguration<Sheet>
	{
		public void Configure(EntityTypeBuilder<Sheet> builder)
		{
			builder.HasMany(sheet => sheet.Materials)
				.WithOne()
				.HasForeignKey(material => material.SheetId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
