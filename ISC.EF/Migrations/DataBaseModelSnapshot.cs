﻿// <auto-generated />
using System;
using ISC.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ISC.EF.Migrations
{
    [DbContext(typeof(DataBase))]
    partial class DataBaseModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CampMentor", b =>
                {
                    b.Property<int>("CampsId")
                        .HasColumnType("int");

                    b.Property<int>("MentorsId")
                        .HasColumnType("int");

                    b.HasKey("CampsId", "MentorsId");

                    b.HasIndex("MentorsId");

                    b.ToTable("CampMentor", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.Camp", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DurationInWeeks")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("OpenForRegister")
                        .HasColumnType("bit");

                    b.Property<int>("Term")
                        .HasColumnType("int");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Camps", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.HeadOfTraining", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CampId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("CampId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("HeadsOfTraining", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.Material", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SheetId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SheetId");

                    b.ToTable("Materials", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.Mentor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("About")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("AccessSessionId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Mentors", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.NewRegistration", b =>
                {
                    b.Property<string>("NationalID")
                        .HasMaxLength(14)
                        .HasColumnType("nvarchar(14)");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("CampId")
                        .HasColumnType("int");

                    b.Property<string>("CodeForceHandle")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("College")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FacebookLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Grade")
                        .HasColumnType("int");

                    b.Property<bool>("HasLaptop")
                        .HasColumnType("bit");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MiddleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VjudgeHandle")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("NationalID");

                    b.HasIndex("CodeForceHandle")
                        .IsUnique();

                    b.ToTable("NewRegistration", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.Session", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CampId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("InstructorName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LocationLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LocationName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Topic")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CampId");

                    b.ToTable("Sessions", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.Sheet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CampId")
                        .HasColumnType("int");

                    b.Property<bool>("IsSohag")
                        .HasColumnType("bit");

                    b.Property<int>("MinimumPrecent")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProblemsCount")
                        .HasColumnType("int");

                    b.Property<string>("SheetCfId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SheetLink")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SheetOrder")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CampId");

                    b.ToTable("Sheets", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.StuffArchive", b =>
                {
                    b.Property<string>("NationalID")
                        .HasMaxLength(14)
                        .HasColumnType("nvarchar(14)");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("CodeForceHandle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("College")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FacebookLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Grade")
                        .HasColumnType("int");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MiddleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VjudgeHandle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("NationalID");

                    b.ToTable("StuffArchives", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.TaskList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<string>("Task")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("traineeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("traineeId");

                    b.ToTable("Tasks", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.Trainee", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CampId")
                        .HasColumnType("int");

                    b.Property<int?>("MentorId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("points")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CampId");

                    b.HasIndex("MentorId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Trainees", (string)null);
                });

            modelBuilder.Entity("ISC.Core.Models.TraineeArchive", b =>
                {
                    b.Property<string>("NationalID")
                        .HasMaxLength(14)
                        .HasColumnType("nvarchar(14)");

                    b.Property<string>("CampName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("CodeForceHandle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("College")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FacebookLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Grade")
                        .HasColumnType("int");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MiddleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VjudgeHandle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("NationalID", "CampName");

                    b.ToTable("TraineesArchives", (string)null);
                });

            modelBuilder.Entity("ISC.Core.ModelsDtos.MentorAttendence", b =>
                {
                    b.Property<int>("MentorId")
                        .HasColumnType("int");

                    b.Property<int>("SessionId")
                        .HasColumnType("int");

                    b.HasKey("MentorId", "SessionId");

                    b.HasIndex("SessionId");

                    b.ToTable("MentorsAttendences", (string)null);
                });

            modelBuilder.Entity("ISC.Core.ModelsDtos.SessionFeedback", b =>
                {
                    b.Property<int>("TraineeId")
                        .HasColumnType("int");

                    b.Property<int>("SessionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Feedback")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("Rate")
                        .HasColumnType("int");

                    b.HasKey("TraineeId", "SessionId");

                    b.HasIndex("SessionId");

                    b.ToTable("SessionsFeedbacks", (string)null);
                });

            modelBuilder.Entity("ISC.Core.ModelsDtos.TraineeAttendence", b =>
                {
                    b.Property<int>("TraineeId")
                        .HasColumnType("int");

                    b.Property<int>("SessionId")
                        .HasColumnType("int");

                    b.HasKey("TraineeId", "SessionId");

                    b.HasIndex("SessionId");

                    b.ToTable("TraineesAttednces", (string)null);
                });

            modelBuilder.Entity("ISC.Core.ModelsDtos.TraineeSheetAccess", b =>
                {
                    b.Property<int>("TraineeId")
                        .HasColumnType("int");

                    b.Property<int>("SheetId")
                        .HasColumnType("int");

                    b.Property<int>("NumberOfProblems")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.HasKey("TraineeId", "SheetId");

                    b.HasIndex("SheetId");

                    b.ToTable("TraineesSheetsAccess", (string)null);
                });

            modelBuilder.Entity("ISC.EF.UserAccount", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("CodeForceHandle")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("College")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FacebookLink")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("nvarchar(7)");

                    b.Property<int>("Grade")
                        .HasColumnType("int");

                    b.Property<DateTime>("JoinDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<DateTime>("LastLoginDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("MiddleName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("NationalId")
                        .IsRequired()
                        .HasMaxLength(14)
                        .HasColumnType("nvarchar(14)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("PhotoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<string>("VjudgeHandle")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("CodeForceHandle")
                        .IsUnique();

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("NationalId")
                        .IsUnique();

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.HasIndex("PhoneNumber")
                        .IsUnique()
                        .HasFilter("PhoneNumber IS NOT NULL");

                    b.HasIndex("VjudgeHandle")
                        .IsUnique()
                        .HasFilter("VjudgeHandle IS NOT NULL");

                    b.ToTable("Accounts", null, t =>
                        {
                            t.HasCheckConstraint("CK_Gender", "Gender in ('Male','Female')");
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("Roles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("UserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("UserTokens", (string)null);
                });

            modelBuilder.Entity("CampMentor", b =>
                {
                    b.HasOne("ISC.Core.Models.Camp", null)
                        .WithMany()
                        .HasForeignKey("CampsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ISC.Core.Models.Mentor", null)
                        .WithMany()
                        .HasForeignKey("MentorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ISC.Core.Models.HeadOfTraining", b =>
                {
                    b.HasOne("ISC.Core.Models.Camp", "Camp")
                        .WithMany("Heads")
                        .HasForeignKey("CampId");

                    b.HasOne("ISC.EF.UserAccount", null)
                        .WithOne("Headofcamp")
                        .HasForeignKey("ISC.Core.Models.HeadOfTraining", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Camp");
                });

            modelBuilder.Entity("ISC.Core.Models.Material", b =>
                {
                    b.HasOne("ISC.Core.Models.Sheet", "Sheet")
                        .WithMany("Materials")
                        .HasForeignKey("SheetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sheet");
                });

            modelBuilder.Entity("ISC.Core.Models.Mentor", b =>
                {
                    b.HasOne("ISC.EF.UserAccount", null)
                        .WithOne("Mentor")
                        .HasForeignKey("ISC.Core.Models.Mentor", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ISC.Core.Models.Session", b =>
                {
                    b.HasOne("ISC.Core.Models.Camp", "Camp")
                        .WithMany("Sessions")
                        .HasForeignKey("CampId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Camp");
                });

            modelBuilder.Entity("ISC.Core.Models.Sheet", b =>
                {
                    b.HasOne("ISC.Core.Models.Camp", "Camp")
                        .WithMany("Sheets")
                        .HasForeignKey("CampId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Camp");
                });

            modelBuilder.Entity("ISC.Core.Models.TaskList", b =>
                {
                    b.HasOne("ISC.Core.Models.Trainee", null)
                        .WithMany("Tasks")
                        .HasForeignKey("traineeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ISC.Core.Models.Trainee", b =>
                {
                    b.HasOne("ISC.Core.Models.Camp", "Camp")
                        .WithMany("Trainees")
                        .HasForeignKey("CampId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ISC.Core.Models.Mentor", "Mentor")
                        .WithMany("Trainees")
                        .HasForeignKey("MentorId");

                    b.HasOne("ISC.EF.UserAccount", null)
                        .WithOne("Trainee")
                        .HasForeignKey("ISC.Core.Models.Trainee", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Camp");

                    b.Navigation("Mentor");
                });

            modelBuilder.Entity("ISC.Core.ModelsDtos.MentorAttendence", b =>
                {
                    b.HasOne("ISC.Core.Models.Mentor", "Mentor")
                        .WithMany("Attendence")
                        .HasForeignKey("MentorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ISC.Core.Models.Session", "Session")
                        .WithMany("MentorsAttendences")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Mentor");

                    b.Navigation("Session");
                });

            modelBuilder.Entity("ISC.Core.ModelsDtos.SessionFeedback", b =>
                {
                    b.HasOne("ISC.Core.Models.Session", "Session")
                        .WithMany("Feedbacks")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ISC.Core.Models.Trainee", "Trainee")
                        .WithMany("SessionsFeedbacks")
                        .HasForeignKey("TraineeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Session");

                    b.Navigation("Trainee");
                });

            modelBuilder.Entity("ISC.Core.ModelsDtos.TraineeAttendence", b =>
                {
                    b.HasOne("ISC.Core.Models.Session", "Session")
                        .WithMany("TraineesAttendences")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ISC.Core.Models.Trainee", "Trainee")
                        .WithMany("TraineesAttendences")
                        .HasForeignKey("TraineeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Session");

                    b.Navigation("Trainee");
                });

            modelBuilder.Entity("ISC.Core.ModelsDtos.TraineeSheetAccess", b =>
                {
                    b.HasOne("ISC.Core.Models.Sheet", "Sheet")
                        .WithMany("TraineesAccessing")
                        .HasForeignKey("SheetId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ISC.Core.Models.Trainee", "Trainee")
                        .WithMany("SheetsAccessing")
                        .HasForeignKey("TraineeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sheet");

                    b.Navigation("Trainee");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("ISC.EF.UserAccount", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("ISC.EF.UserAccount", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ISC.EF.UserAccount", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("ISC.EF.UserAccount", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ISC.Core.Models.Camp", b =>
                {
                    b.Navigation("Heads");

                    b.Navigation("Sessions");

                    b.Navigation("Sheets");

                    b.Navigation("Trainees");
                });

            modelBuilder.Entity("ISC.Core.Models.Mentor", b =>
                {
                    b.Navigation("Attendence");

                    b.Navigation("Trainees");
                });

            modelBuilder.Entity("ISC.Core.Models.Session", b =>
                {
                    b.Navigation("Feedbacks");

                    b.Navigation("MentorsAttendences");

                    b.Navigation("TraineesAttendences");
                });

            modelBuilder.Entity("ISC.Core.Models.Sheet", b =>
                {
                    b.Navigation("Materials");

                    b.Navigation("TraineesAccessing");
                });

            modelBuilder.Entity("ISC.Core.Models.Trainee", b =>
                {
                    b.Navigation("SessionsFeedbacks");

                    b.Navigation("SheetsAccessing");

                    b.Navigation("Tasks");

                    b.Navigation("TraineesAttendences");
                });

            modelBuilder.Entity("ISC.EF.UserAccount", b =>
                {
                    b.Navigation("Headofcamp");

                    b.Navigation("Mentor");

                    b.Navigation("Trainee");
                });
#pragma warning restore 612, 618
        }
    }
}
