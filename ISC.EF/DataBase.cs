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
		public DataBase():base() { }
		public DataBase(DbContextOptions<DataBase> options) : base(options)
		{

		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
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
			//new MentorConfiguration().Configure(builder.Entity<Mentor>());
			//new CampConfiguration().Configure(builder.Entity<Camp>());
			new MentorAttendenceConfigurations().Configure(builder.Entity<MentorAttendence>());
			new MentorOfCampConfigurations().Configure(builder.Entity<MentorOfCamp>());
			new NewRegisterationConfigurations().Configure(builder.Entity<NewRegitseration>());
			new SessionFeedbackConfigurations().Configure(builder.Entity<SessionFeedback>());
			//new SheetConfigurations().Configure(builder.Entity<Sheet>());
			new StuffArchiveConfigurations().Configure(builder.Entity<StuffArchive>());
			new TraineeArchiveConfigurations().Configure(builder.Entity<TraineeArchive>());
			new TraineeAttendenceConfigurations().Configure(builder.Entity<TraineeAttendence>());
			//new TraineeConfigurations().Configure(builder.Entity<Trainee>());
			new TraineeSheetAccessConfigurations().Configure(builder.Entity<TraineeSheetAccess>());
		}
		public DbSet<Trainee> Trainees { get; set; }
		public DbSet<Session> Sessions { get; set; }
		public DbSet<TraineeAttendence> TraineesAttednces { get; set; }
		public DbSet<Sheet> Sheets { get; set; }
		public DbSet<TraineeSheetAccess> TraineesSheetsAccess { get; set; }
		public DbSet<Mentor> Mentors { get; set; }
		public DbSet<Camp> Camps { get; set; }
		public DbSet<SessionFeedback> SessionsFeedbacks { get; set; }
		public DbSet<MentorOfCamp> MentorsOfCamps { get; set; }
		public DbSet<MentorAttendence> MentorsAttendences { get; set; }
		public DbSet<Material> Materials { get; set; }
		public DbSet<TraineeArchive> TraineesArchives { get; set; }
		public DbSet<StuffArchive> StuffArchives { get; set; }
		public DbSet<NewRegitseration> NewRegitserations { get; set; }
		public DbSet<HeadOfTraining> HeadsOfTraining { get; set;}
	}
}
