using ISC.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace ISC.Services.Services.ExceptionSerivces
{
	public class ExceptionMiddleWareService
	{
		private readonly RequestDelegate _next;
		private static JsonSerializerOptions? _jsonOptions;

		public ExceptionMiddleWareService(RequestDelegate next)
		{
			_next = next;
			_jsonOptions = new JsonSerializerOptions()
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
				if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
				{

					var response = new ServiceResponse<bool>();
					response.IsSuccess = false;
					response.Comment = "Unauthorized";

					var exceptionResult = JsonSerializer.Serialize(response,_jsonOptions);
					context.Response.ContentType = "application/json";
					context.Response.StatusCode = (int)HttpStatusCode.OK;
					await context.Response.WriteAsync(exceptionResult);

					return;
				}

			}
			catch (Exception ex)
			{
				await HandleException(context, ex);
			}
		}
		private static Task HandleException(HttpContext context, Exception ex)
		{
			var response = new ServiceResponse<string>();

			response.IsSuccess = false;
			response.Comment = ex.Message;
			response.Entity = ex.GetType().ToString().Split('.').Last();
			var exceptionResult = JsonSerializer.Serialize(response, _jsonOptions);
			context.Response.ContentType = "application/json";
			context.Response.StatusCode= (int)HttpStatusCode.OK;

			return context.Response.WriteAsync(exceptionResult);
		}
	}
}
