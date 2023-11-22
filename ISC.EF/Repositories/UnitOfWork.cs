using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ISC.EF.Repositories
{
    public class UnitOfWork : IUnitOfWork
	{
		private readonly UserManager<UserAccount> _UserManager;
		public readonly DataBase _DataBase;
		private readonly IWebHostEnvironment _Host;
		private readonly IHttpContextAccessor _HttpContext;
		public ITraineeRepository Trainees { get; private set; }

		public IBaseRepository<Session> Sessions { get; private set; }

		public IMentorRepository Mentors { get; private set; }

		public ITraineeAttendenceRepository TraineesAttendence { get; private set; }

		public IBaseRepository<Sheet> Sheets { get; private set; }

		public IBaseRepository<TraineeSheetAccess> TraineesSheetsAccess { get; private set; }

		public IHeadOfCampRepository HeadofCamp { get; private set; }

		public ICampRepository Camps { get; private set; }

		public ISessionFeedbackRepository SessionsFeedbacks { get; private set; }


		public IBaseRepository<Material> Materials { get; private set; }

		public IBaseRepository<TraineeArchive> TraineesArchive { get; private set; }

		public IBaseRepository<StuffArchive> StuffArchive { get; private set; }

		public IBaseRepository<NewRegistration> NewRegitseration { get; private set; }
        public UnitOfWork(DataBase database,UserManager<UserAccount> usermanager,IWebHostEnvironment host, IHttpContextAccessor httpContext)
        {
			_DataBase = database;
			_UserManager = usermanager;
			Trainees = new TraineeRepository(_DataBase,_UserManager);
			Sessions= new BaseRepository<Session>(_DataBase);
			Mentors = new MentorRepository(database,usermanager);
			TraineesAttendence = new TraineeAttendenceRepository(_DataBase);
			Sheets = new BaseRepository<Sheet>(_DataBase);
			TraineesSheetsAccess = new BaseRepository<TraineeSheetAccess>(_DataBase);
			HeadofCamp = new HeadOfCampRepository(_DataBase);
			Camps = new CampRepository(_DataBase);
			SessionsFeedbacks = new SessionFeedbackRepository(_DataBase);
			Materials = new BaseRepository<Material>(_DataBase);
			TraineesArchive = new BaseRepository<TraineeArchive>(_DataBase);
			StuffArchive = new BaseRepository<StuffArchive>(_DataBase);
			NewRegitseration = new BaseRepository<NewRegistration>(_DataBase);
		}
		public async Task<bool> addToRoleAsync<T>(T account, string role,int?campId,int?mentorId)
		{
			if(account is UserAccount Acc)
			{
				if (Acc != null && _UserManager.GetRolesAsync(Acc).Result.Contains(role) == true)
					return true;
				else if(Acc==null)
					return false;
				try
				{
					if (role == Role.TRAINEE)
					{
						if (campId == null)
						{
							return false;
						}
						await _UserManager.AddToRoleAsync(Acc, role);
						Trainee Trainee;
						if (mentorId != null)
							Trainee = new Trainee() { UserId = Acc.Id, CampId = (int)campId, MentorId = mentorId };
						else
							Trainee = new Trainee() { UserId = Acc.Id, CampId = (int)campId };
						Trainees.addAsync(Trainee);
					}
					else if (role == Role.MENTOR)
					{
						await _UserManager.AddToRoleAsync(Acc, role);
						Mentor mentor = new Mentor() { UserId = Acc.Id };
						Mentors.addAsync(mentor);
						await _DataBase.SaveChangesAsync();

						if(campId is not null)
						{
							var camp = await _DataBase.Camps.SingleAsync(c => c.Id == campId);
							mentor.Camps = new List<Camp>() { camp };
							_DataBase.Update(mentor);
						}
					}
					else if (role == Role.HOC && campId!=null)
					{
						await _UserManager.AddToRoleAsync(Acc, role);
						HeadOfTraining HeadOfTraining = new HeadOfTraining() { UserId = Acc.Id, CampId = campId };
						HeadofCamp.addAsync(HeadOfTraining);
					}
					else if (role == Role.LEADER || role == Role.INSTRUCTOR)
					{
						await _UserManager.AddToRoleAsync(Acc, role);
					}
					else
					{
						return false;
					}
					return true;
				}
				catch
				{
					return false;
				}
			}
			else
			{
				return false;
			}
			

		}
		protected async Task<string> addMediaAsync(IFormFile media)
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
			}
			if (isImage)
				return @$"{_HttpContext.HttpContext?.Request.Scheme}://{_HttpContext?.HttpContext?.Request.Host}/Images/" + FileName + Extension;
			else
				return @$"{_HttpContext.HttpContext?.Request.Scheme}://{_HttpContext?.HttpContext?.Request.Host}/Records/" + FileName + Extension;
		}
		protected async Task<bool> deleteMediaAsync(string url)
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
		public async Task<string?> getMediaAsync(IFormFile media)
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
		protected async Task<string?> updateMedia(string oldUrl, IFormFile newMedia)
		{
			var Result = getMediaAsync(newMedia).Result;
			if (Result == null) return Result;
			if (oldUrl == Result) return oldUrl;
			if (!await deleteMediaAsync(oldUrl)) return null;
			var NewUrl = await addMediaAsync(newMedia);
			if (NewUrl == null) return null;
			return NewUrl;
		}
		private bool isImageExtension(string extension)
		{
			return extension == ".jpg" || extension == ".jpeg" || extension == ".jpe";
		}
		public async Task<int> completeAsync()
		{
			return await _DataBase.SaveChangesAsync();
		}
        public void Dispose()
		{
			_DataBase.Dispose();
		}
	}
}
