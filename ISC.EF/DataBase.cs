using ISC.Core.Models;
using ISC.EF.ModelsConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISC.Core.ModelsDtos;
namespace ISC.EF
{
	public class DataBase :IdentityDbContext<UserAccount>
	{
		public DataBase(DbContextOptions<DataBase> options):base(options)
		{

		}
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			builder.Entity<UserAccount>().ToTable("Accounts");
			builder.Entity<IdentityRole>().ToTable("Roles");
			builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
			builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
			builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
			builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
			builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
			new UserAccountconfiguration().Configure(builder.Entity<UserAccount>());
			new MentorConfiguration().Configure(builder.Entity<Mentor>());
			//builder.Entity<SessionFeedback>().Property(i => i.DateTime).HasDefaultValueSql("Select GETDATE()");
			//builder.Entity<SessionFeedback>().HasKey(i => new { i.SessionId, i.TraineeId });
			//builder.Entity<TraineeArchive>().HasKey(i=>i.);
			//builder.Entity<StuffArchive>().HasKey(i => i.id);
			//builder.Entity<StuffArchive>().Property(e => e.id).ValueGeneratedNever();
		}
		//DbSet<SessionFeedback> sessions { get; set; }
		//DbSet<TraineeArchive> traineesArchives { get; set; }
		//DbSet<StuffArchive> stuffArchives { get; set; }
	}
}
