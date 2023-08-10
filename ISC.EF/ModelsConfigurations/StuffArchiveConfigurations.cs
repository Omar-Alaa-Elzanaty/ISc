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
	internal class StuffArchiveConfigurations : IEntityTypeConfiguration<StuffArchive>
	{
		public void Configure(EntityTypeBuilder<StuffArchive> builder)
		{
			builder.HasKey(stuff => stuff.NationalID);
			builder.Property(stuff => stuff.NationalID).HasMaxLength(14);
		}
	}
}
