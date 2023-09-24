using ISC.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace ISC.EF.ModelsConfigurations
{
	internal class MentorConfiguration : IEntityTypeConfiguration<Mentor>
	{
		public void Configure(EntityTypeBuilder<Mentor> builder)
		{
		}
	}
}
