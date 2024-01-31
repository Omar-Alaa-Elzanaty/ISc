using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.ISerivces
{
	public interface IMediaServices
	{
		Task<string> AddAsync(IFormFile media);
		Task<bool> DeleteAsync(string? url);
		Task<string?> Update(string? oldUrl, IFormFile newMedia);
	}
}
