using Quartz;

namespace ISC.API.Helpers
{
	public  static class DepedencyInjectionSchedule
	{
		public static void AddInfrastructure(this IServiceCollection services)
		{
			services.AddQuartz(options =>
			{
				options.UseMicrosoftDependencyInjectionJobFactory();
				var jobkey = JobKey.Create(nameof(ScheduleSetting));
				options.AddJob<ScheduleSetting>(jobkey)
				.AddTrigger(t => 
							t.ForJob(jobkey)
							.WithSimpleSchedule(s=>
							s.WithIntervalInHours(1).RepeatForever())
				);
			});
			services.AddQuartzHostedService();
		} 
	}
}
