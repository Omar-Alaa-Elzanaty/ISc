using ISC.Services.Services.ExceptionSerivces;
using System.Runtime.CompilerServices;

namespace ISC.API
{
	public static class GlobalHandleExceptionMiddleWare
	{
		public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
			=> app.UseMiddleware<ExceptionMiddleWareService>();
	}
}
