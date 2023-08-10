﻿using ISC.Core.Models;
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
			builder.HasIndex(acc => acc.VjudgeHandle).IsUnique().HasFilter(null);
			builder.HasIndex(acc => acc.CodeForceHandle).IsUnique();
			builder.HasIndex(acc => acc.Email).IsUnique();
			builder.HasIndex(acc => acc.PhoneNumber).IsUnique().HasFilter(null);
			builder.Property(i => i.UserName).HasMaxLength(20);
			builder.Property(i => i.FirstName).HasMaxLength(20);
			builder.Property(i => i.LastName).HasMaxLength(20);
			builder.Property(i => i.College).HasMaxLength(20);
			builder.Property(i => i.Gender).HasMaxLength(7);
			builder.Property(i => i.NationalId).HasMaxLength(14);
			builder.Property(i => i.FacebookLink).HasMaxLength(100);
			builder.Property(i => i.Email).IsRequired();
			builder.Property(acc => acc.VjudgeHandle).IsRequired(false);
			builder.Property(acc => acc.PhoneNumber).IsRequired(false);
			builder.Property(acc => acc.JoinDate).HasDefaultValueSql("GETDATE()");
			builder.ToTable(b=>b.HasCheckConstraint("CK_Gender","Gender in ('Male','Female')"));
			builder.HasOne(c => c.Trainee).WithOne().HasForeignKey<Trainee>(t=>t.UserId);
			builder.HasOne(c => c.Mentor).WithOne().HasForeignKey<Mentor>(m=>m.UserId);

		}
	}
}
