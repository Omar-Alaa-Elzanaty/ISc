using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services
{
	public class MediaServices:IMediaServices
	{
		private readonly IWebHostEnvironment _Host;
		private readonly IHttpContextAccessor _HttpContext;

		public MediaServices(IWebHostEnvironment host, IHttpContextAccessor httpContext)
		{
			_Host = host;
			_HttpContext = httpContext;
		}
		public async Task<string> AddAsync(IFormFile media)
		{
			string RootPath = _Host.WebRootPath;
			string FileName = Guid.NewGuid().ToString();
			string Extension = Path.GetExtension(media.FileName);
			string MediaFolderPath = "";
			bool isImage = false;
			if (isImageExtension(Extension))
			{
				MediaFolderPath = Path.Combine(RootPath, "Images");
				isImage = true;
			}
			using (FileStream fileStreams = new(Path.Combine(MediaFolderPath,
											FileName + Extension), FileMode.Create))
			{
				media.CopyTo(fileStreams);
				fileStreams.Flush();
				fileStreams.Dispose();
				fileStreams.Close();
			}
			if (isImage)
				return @$"{_HttpContext.HttpContext?.Request.Scheme}://{_HttpContext?.HttpContext?.Request.Host}/Images/" + FileName + Extension;
			else
				return @$"{_HttpContext.HttpContext?.Request.Scheme}://{_HttpContext?.HttpContext?.Request.Host}/Records/" + FileName + Extension;
		}
		public async Task<bool> DeleteAsync(string url)
		{
			try
			{
				string RootPath = _Host.WebRootPath.Replace("\\\\", "\\");
				var imageNameToDelete = Path.GetFileNameWithoutExtension(url);
				var EXT = Path.GetExtension(url);
				string? oldPath = "";
				if (isImageExtension(EXT))
					oldPath = $@"{RootPath}\Images\{imageNameToDelete}{EXT}";
				else return false;
				if (File.Exists(oldPath))
				{
					File.Delete(oldPath);
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}
		public async Task<string?> Update(string oldUrl, IFormFile newMedia)
		{
			var Result = GetPathAsync(newMedia).Result;
			if (Result == null) return Result;
			if (oldUrl == Result) return oldUrl;
			var NewUrl = await AddAsync(newMedia);
			if (NewUrl == null)
			{
				throw new Exception("Coudln't update photo");

			}
			await DeleteAsync(oldUrl);
			return NewUrl;
		}
		private async Task<string?> GetPathAsync(IFormFile media)
		{
			if (media == null) return null;
			string FileName = Guid.NewGuid().ToString();
			string Extension = Path.GetExtension(media.FileName);
			if (isImageExtension(Extension))
			{
				return @$"{_HttpContext.HttpContext?.Request.Scheme}://{_HttpContext?.HttpContext?.Request.Host}/Images/" + FileName + Extension;
			}
			else
			{
				return null;
			}
		}
		private bool isImageExtension(string extension)
		{
			return extension == ".jpg" || extension == ".jpeg" || extension == ".jpe";
		}
	}
}
