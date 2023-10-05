using ISC.Core.Interfaces;
using ISC.EF;
using ISC.EF.Repositories;
using ISC.Services.ISerivces;
using ISC.Services.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.ComponentModel;
using System.Threading;

namespace ISC.Services.Helpers
{
	public class ScheduleSetting : IJob
	{
		private readonly ILogger<ScheduleSetting> _Logger;
		private ScheduleSerives _Services;

		public ScheduleSetting(ILogger<ScheduleSetting>logger, IUnitOfWork unitofwork, IOnlineJudgeServices onlinejudge, UserManager<UserAccount> usermanger,IOptions<CodeForceConnection>cfconnection)
        {
			this._Logger = logger;
			_Services=new ScheduleSerives(unitofwork, onlinejudge,usermanger,cfconnection);
		}
		public async Task Execute(IJobExecutionContext context)
		{
			await _Services.updateTraineeSolveCurrentAccessAsync();
		}
	}
}