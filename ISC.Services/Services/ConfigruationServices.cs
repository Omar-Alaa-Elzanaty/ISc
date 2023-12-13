using ISC.Core.Interfaces;
using ISC.EF;
using ISC.EF.Repositories;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using ISC.Services.Services.ModelSerivces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
namespace ISC.Services.Services
{
	public static class ConfigruationServices
	{
		public static void AddServices(this IServiceCollection services)
		{
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<IMentorRepository, MentorRepository>();
			services.AddScoped<IAuthanticationServices, AuthanticationServices>();
			services.AddScoped<IMailServices, MailServices>();
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IOnlineJudgeServices, CodeforceApiService>();
			services.AddScoped<ISheetServices, SheetServices>();
			services.AddScoped<ISessionsServices, SesionsServices>();
			services.AddScoped<IDataSeeding, DataSeeding>();
			services.AddScoped<ILeaderServices, LeaderServices>();
			services.AddScoped<ICampServices, CampServices>();
			services.AddScoped<IHeadSerivces, HeadServices>();
			services.AddScoped<ITraineeService, TraineeService>();
		}
	}
}
