using ISC.Core.Interfaces;
using ISC.EF;
using ISC.EF.Repositories;
using ISC.Services.ISerivces;
using Microsoft.Extensions.DependencyInjection;
namespace ISC.Services.Services
{
	public static class ConfigruationServices
	{
		public static void addServices(this IServiceCollection services)
		{
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<IMentorRepository, MentorRepository>();
			services.AddScoped<IAuthanticationServices, AuthanticationServices>();
			services.AddScoped<IMailServices, MailServices>();
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IOnlineJudgeServices, CodeforceApiService>();
		}
	}
}
