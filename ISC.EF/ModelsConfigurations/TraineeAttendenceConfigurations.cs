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
	internal class TraineeAttendenceConfigurations : IEntityTypeConfiguration<TraineeAttendence>
	{
		public void Configure(EntityTypeBuilder<TraineeAttendence> builder)
		{
			builder.HasOne(attend => attend.Trainee)
					.WithMany(trainee => trainee.TraineesAttendences)
					.HasForeignKey(attend => attend.TraineeId)
					.OnDelete(DeleteBehavior.Cascade);
			builder.HasOne(attend => attend.Session)
				.WithMany(session => session.TraineesAttendences)
				.HasForeignKey(attend => attend.SessionId)
				.OnDelete(DeleteBehavior.Restrict);
			builder.HasKey(attend => new { attend.TraineeId, attend.SessionId });
		}
	}
}
