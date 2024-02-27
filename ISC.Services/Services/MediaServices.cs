using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services
{
	public class MediaServices:IMediaServices
	{
		private readonly IWebHostEnvironment _host;
		private readonly HttpContext _httpContext;

		public MediaServices(IWebHostEnvironment host, IHttpContextAccessor context)
		{
			_host = host;
			_httpContext = context.HttpContext;
		}
		public async Task<string> AddAsync(IFormFile media)
		{
            var extension = Path.GetExtension(media.FileName).ToLower();

            var uniqueFileName = Guid.NewGuid().ToString() + extension;

            var uploadsFolder = Path.Combine("wwwroot", "Images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                media.CopyTo(stream);
                stream.Dispose();
            }
            var baseUrl = @$"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}/Images/";

            return baseUrl + uniqueFileName;
   //         string RootPath = _Host.WebRootPath;
			//string FileName = Guid.NewGuid().ToString();
			//string Extension = Path.GetExtension(media.FileName);
			//string MediaFolderPath = "";
			//bool isImage = false;
			//if (isImageExtension(Extension))
			//{
			//	MediaFolderPath = Path.Combine(RootPath, "Images");
			//	isImage = true;
			//}
			//using (FileStream fileStreams = new(Path.Combine(MediaFolderPath,
			//								FileName + Extension), FileMode.Create))
			//{
			//	media.CopyTo(fileStreams);
			//	fileStreams.Flush();
			//	fileStreams.Dispose();
			//	fileStreams.Close();
			//}
			//if (isImage)
			//	return @$"{_httpContext.HttpContext?.Request.Scheme}://{_httpContext?.HttpContext?.Request.Host}/Images/" + FileName + Extension;
			//else
			//	return @$"{_httpContext.HttpContext?.Request.Scheme}://{_httpContext?.HttpContext?.Request.Host}/Records/" + FileName + Extension;
		}
		public async Task<bool> DeleteAsync(string? url)
		{
			if(url == null)
			{
				return true;
			}
			try
			{
                string RootPath = _host.WebRootPath.Replace("\\\\", "\\");
                var imageNameToDelete = Path.GetFileNameWithoutExtension(url);
                var EXT = Path.GetExtension(url);
                var oldImagePath = $@"{RootPath}\Images\{imageNameToDelete}{EXT}";
                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                }
                await Task.CompletedTask;
				return true;
            }
			catch
			{
				return false;
			}
		}
		public async Task<string?> Update(string oldUrl, IFormFile newMedia)
		{
            await DeleteAsync(oldUrl);
            return await AddAsync(newMedia);
        }
		//private async Task<string?> GetPathAsync(IFormFile media)
		//{
		//	if (media == null) return null;
		//	string FileName = Guid.NewGuid().ToString();
		//	string Extension = Path.GetExtension(media.FileName);
		//	if (isImageExtension(Extension))
		//	{
		//		return @$"{_httpContext.HttpContext?.Request.Scheme}://{_httpContext?.HttpContext?.Request.Host}/Images/" + FileName + Extension;
		//	}
		//	else
		//	{
		//		return null;
		//	}
		//}
		private bool isImageExtension(string extension)
		{
			return extension == ".jpg" || extension == ".jpeg" || extension == ".jpe";
		}
	}
}
