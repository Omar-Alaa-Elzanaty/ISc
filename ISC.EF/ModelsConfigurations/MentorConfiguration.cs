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
	public class MentorConfiguration : IEntityTypeConfiguration<Mentor>
	{
		public void Configure(EntityTypeBuilder<Mentor> builder)
		{
			builder.HasMany(m => m.Trainees)
				.WithOne()
				.HasForeignKey(m => m.MentorId);
				//.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
