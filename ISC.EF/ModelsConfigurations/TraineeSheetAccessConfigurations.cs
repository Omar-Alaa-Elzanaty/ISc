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
	internal class TraineeSheetAccessConfigurations : IEntityTypeConfiguration<TraineeSheetAccess>
	{
		public void Configure(EntityTypeBuilder<TraineeSheetAccess> builder)
		{
			builder.HasOne(access => access.Trainee)
				   .WithMany(trainee => trainee.SheetsAccessing)
				   .HasForeignKey(access => access.TraineeId)
				   .OnDelete(DeleteBehavior.Cascade);
			builder.HasOne(access => access.Sheet)
				   .WithMany(sheet => sheet.TraineesAccessing)
				   .HasForeignKey(access => access.SheetId)
				   .OnDelete(DeleteBehavior.Restrict);
			builder.HasKey(access => new {access.TraineeId, access.SheetId});
		}
	}
}
