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
	internal class TaskListConfiguration : IEntityTypeConfiguration<TaskList>
	{
		public void Configure(EntityTypeBuilder<TaskList> builder)
		{
			builder.HasOne<Trainee>()
				.WithMany(t => t.Tasks)
				.HasForeignKey(t => t.traineeId);
		}
	}
}
