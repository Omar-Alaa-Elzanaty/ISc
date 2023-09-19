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
	internal class UserAccountconfiguration : IEntityTypeConfiguration<UserAccount>
	{
		public void Configure(EntityTypeBuilder<UserAccount> builder)
		{
			builder.HasIndex(acc => acc.NationalId).IsUnique();
			builder.HasIndex(acc => acc.VjudgeHandle).HasFilter("VjudgeHandle IS NOT NULL").IsUnique();
			builder.HasIndex(acc => acc.CodeForceHandle).IsUnique();
			builder.HasIndex(acc => acc.Email).IsUnique();
			builder.HasIndex(acc => acc.PhoneNumber).HasFilter("PhoneNumber IS NOT NULL").IsUnique();
			builder.Property(i => i.UserName).HasMaxLength(40);
			builder.Property(i => i.FirstName).HasMaxLength(20);
			builder.Property(i => i.MiddleName).HasMaxLength(20);
			builder.Property(i => i.LastName).HasMaxLength(20);
			builder.Property(i => i.College).HasMaxLength(30);
			builder.Property(i => i.Gender).HasMaxLength(7);
			builder.Property(i => i.NationalId).HasMaxLength(14);
			builder.Property(i => i.FacebookLink).HasMaxLength(200);
			builder.Property(i => i.Email).IsRequired();
			builder.Property(acc => acc.JoinDate).HasDefaultValueSql("GETDATE()");
			builder.ToTable(b=>b.HasCheckConstraint("CK_Gender","Gender in ('Male','Female')"));
			builder.HasOne(c => c.Trainee).WithOne().HasForeignKey<Trainee>(t=>t.UserId);
			builder.HasOne(c => c.Mentor).WithOne().HasForeignKey<Mentor>(m=>m.UserId);
			builder.HasOne(c=>c.Headofcamp).WithOne().HasForeignKey<HeadOfTraining>(h=>h.UserId);
		}
	}
}
