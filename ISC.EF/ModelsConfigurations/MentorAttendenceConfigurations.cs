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
	internal class MentorAttendenceConfigurations : IEntityTypeConfiguration<MentorAttendence>
	{
		public void Configure(EntityTypeBuilder<MentorAttendence> builder)
		{
			builder.HasOne(attend => attend.Mentor)
				   .WithMany(mentor => mentor.Attendence)
				   .HasForeignKey(attend => attend.MentorId)
				   .OnDelete(DeleteBehavior.Cascade);
			builder.HasOne(attend => attend.Session)
				   .WithMany(session => session.MentorsAttendences)
				   .HasForeignKey(attend => attend.SessionId)
				   .OnDelete(DeleteBehavior.Restrict);
			builder.HasKey(attend => new { attend.MentorId, attend.SessionId });
		}
	}
}
