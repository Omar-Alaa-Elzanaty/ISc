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
	internal class SessionFeedbackConfigurations : IEntityTypeConfiguration<SessionFeedback>
	{
		public void Configure(EntityTypeBuilder<SessionFeedback> builder)
		{
			builder.HasOne(feed => feed.Trainee)
				   .WithMany(trainee => trainee.SessionsFeedbacks)
				   .HasForeignKey(feed => feed.TraineeId)
				   .OnDelete(DeleteBehavior.Cascade);
			builder.HasOne(feed => feed.Session)
				   .WithMany(session => session.Feedbacks)
				   .HasForeignKey(feed => feed.SessionId)
				   .OnDelete(DeleteBehavior.Restrict);
			builder.HasKey(feed => new {feed.TraineeId, feed.SessionId});
			builder.Property(i => i.DateTime).HasDefaultValueSql("GETDATE()");
			builder.Property(i => i.Feedback).HasMaxLength(500);
		}
	}
}
