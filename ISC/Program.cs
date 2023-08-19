using ISC.API.Helpers;
using ISC.API.ISerivces;
using ISC.API.Services;
using ISC.Core.Interfaces;
using ISC.EF;
using ISC.EF.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ISC
{
    public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddScoped<IAuthanticationServices, AuthanticationServices>();
			builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
			//builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
			builder.Services.AddDbContext<DataBase>(option =>
				option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
									b => b.MigrationsAssembly(typeof(DataBase).Assembly.FullName))
									);
			builder.Services.AddIdentity<UserAccount, IdentityRole>()
				.AddEntityFrameworkStores<DataBase>()
				.AddDefaultTokenProviders();
			builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
			builder.Services.AddScoped<IMailServices, MailServices>();
			builder.Services.AddScoped<IAccountRepository, AccountRepository>();
			builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));
			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(o =>
			{
				o.RequireHttpsMetadata = false;
				o.SaveToken = false;
				o.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidIssuer = builder.Configuration["JWT:Issuer"],
					ValidAudience = builder.Configuration["JWT:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
					ClockSkew = TimeSpan.Zero
				};
			});

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}