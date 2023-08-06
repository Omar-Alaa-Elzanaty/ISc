using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.ModelsConfigurations
{
	internal class UserAccountconfiguration : IEntityTypeConfiguration<UserAccount>
	{
		public void Configure(EntityTypeBuilder<UserAccount> builder)
		{
			builder.HasIndex(acc => acc.NationalId);
			builder.HasIndex(acc => acc.VjudgeHandle);
			builder.HasIndex(acc => acc.CodeForceHandle);
			builder.HasIndex(acc => acc.FacebookLink);
			builder.HasIndex(acc => acc.Email);
			builder.HasIndex(acc => acc.PhoneNumber);
		}
	}
}
