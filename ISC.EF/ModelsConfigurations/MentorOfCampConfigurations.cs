using ISC.Core.ModelsDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.ModelsConfigurations
{
	internal class MentorOfCampConfigurations : IEntityTypeConfiguration<MentorOfCamp>
	{
		public void Configure(EntityTypeBuilder<MentorOfCamp> builder)
		{
			builder.HasOne(mc => mc.Camp)
				   .WithMany(camp => camp.MentorsOfCamp)
				   .HasForeignKey(mc => mc.CampId)
				   .OnDelete(DeleteBehavior.Restrict);
			builder.HasOne(mc => mc.Mentor)
				   .WithMany(mentor => mentor.Camps)
				   .HasForeignKey(mc => mc.MentorId)
				   .OnDelete(DeleteBehavior.Cascade);
			builder.HasKey(mc => new {mc.MentorId, mc.CampId});
		}
	}
}
