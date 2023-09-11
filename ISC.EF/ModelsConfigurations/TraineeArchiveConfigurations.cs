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
	internal class TraineeArchiveConfigurations : IEntityTypeConfiguration<TraineeArchive>
	{
		public void Configure(EntityTypeBuilder<TraineeArchive> builder)
		{
			builder.HasKey(trainee => new { trainee.NationalID,trainee.CampName});
			builder.Property(trainee => trainee.NationalID).HasMaxLength(14);
		}
	}
}
