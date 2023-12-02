using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.EF;
using ISC.EF.Repositories;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ISC.Services.Services.ModelSerivces
{
    public class HeadServices:IHeadSerivces
    {
		private readonly UserManager<UserAccount> _userManager;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOnlineJudgeServices _onlineJudgeServices;
		private readonly CodeForceConnection _codeForceConnection;

		public HeadServices(UserManager<UserAccount> userManager,
			IUnitOfWork unitofwork,
			IOnlineJudgeServices onlineJudgeServices,
			IOptions<CodeForceConnection> connection)
		{
			_userManager = userManager;
			_unitOfWork = unitofwork;
			_onlineJudgeServices = onlineJudgeServices;
			_codeForceConnection = connection.Value;
		}

		public async Task<ServiceResponse<List<TraineeMentorDto>>> DisplayTraineeMentorAsync(string userId)
		{
			var response = new ServiceResponse<List<TraineeMentorDto>>();

			var campId = _unitOfWork.HeadofCamp.findWithChildAsync(h => h.UserId == userId, new[] { "Camp" }).Result?.CampId;

			var trainees =await _userManager.Users
				.Include(u => u.Trainee)
				.Where(u => u.Trainee != null && u.Trainee.CampId == campId)
				.ToListAsync();

			var mentors = await _userManager.Users
				.Include(u => u.Mentor)
				.Where(u => u.Mentor != null)
				.ToListAsync();

			var traineeMentor = trainees.Join(mentors, t => t.Trainee.MentorId, m => m.Mentor.Id,
				(t, m) => new TraineeMentorDto()
				{
					TraineeId = t.Trainee.Id,
					MentorId = m.Mentor?.Id,
					TraineeName = t.FirstName + ' ' + t.MiddleName + " " + t.LastName,
					MentorName = m.Mentor != null ? m.FirstName + ' ' + m.MiddleName + " " + m.LastName : null
				}).ToList();

			response.Success = true;
			response.Entity = traineeMentor;

			if (traineeMentor.IsNullOrEmpty())
			{
				response.Entity = new List<TraineeMentorDto>();
			}

			return response;
		}
		public async Task SubmitTraineeMentorAsync(List<AssignTraineeMentorDto> data)
		{
			foreach(var item in data)
			{
				var trainee = await _unitOfWork.Trainees.getByIdAsync(item.TraineeId);
				if(trainee is null)
				{
					continue;
				}
				var mentor = await _unitOfWork.Mentors.getByIdAsync(item.MentorId);
				trainee.Mentor= mentor;
				trainee.MentorId = item.MentorId;
				await _unitOfWork.Trainees.UpdateAsync(trainee);
			}
			await _unitOfWork.completeAsync();
		}
		public async Task<ServiceResponse<TraineeSheetAcessDto>> DisplayTraineeAccess(int campId)
		{
			var response=new ServiceResponse<TraineeSheetAcessDto>() { Success = true };

			var trainees = _userManager.Users
						.Include(u => u.Trainee)
						.Where(u => u.Trainee != null && u.Trainee.CampId == campId)
						.Select(u => new
						{
							traineeId = u.Trainee.Id,
							FullName = u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName
						});

			var sheets= await _unitOfWork.Sheets.Get().
						Where(s=>s.CampId==campId)
						.Select(s => new
						{
							s.Id,
							s.Name
						}).ToListAsync();

			List<TraineeSheetAccess> dbSheetAccess = new List<TraineeSheetAccess>();
			response.Entity = new TraineeSheetAcessDto();

			if (sheets is null)
			{
				return response;
			}

			dbSheetAccess = await _unitOfWork.TraineesSheetsAccess
						.Get()
						.Where(ac => sheets.Any(s => s.Id == ac.SheetId))
						.ToListAsync();

			response.Entity.Sheets = sheets.Select(s => new SheetsInfo()
			{
				Id = s.Id,
				Name = s.Name,
			}).ToList();

			//loop on trainee / find trainee with acces / add to status 
			foreach(var trainee in trainees)
			{
				TraineeAccess access=new TraineeAccess();
				access.FullName = trainee.FullName;

				foreach(var sheet in sheets)
				{
					access.Statues.Add(new Access()
					{
						SheetId = sheet.Id,
						HasAccess = dbSheetAccess.Any(da => da.TraineeId == trainee.traineeId && da.SheetId == sheet.Id)
					});
				}
				response.Entity.TraineeAccess.Add(access);
			}

			return response;
		}
		public async Task AddNewTrainingSheetAccess(int sheetId,int campId)
		{
			if (campId == 0)
			{
				throw new KeyNotFoundException("Unvalid campid");
			}

			var accessing = await _unitOfWork.Trainees.Get()
							.Where(t => t.CampId == campId)
							.Select(t => new TraineeSheetAccess()
							{
								TraineeId = t.Id,
								SheetId = sheetId,
							}).ToListAsync();
			await _unitOfWork.TraineesSheetsAccess.AddGroup(accessing);
		}
		public async Task<List<TraineeStandingDto>> GeneralStandingsAsync(int? campId)
		{

			if (campId is null)
			{
				return new List<TraineeStandingDto>();
			}

			var response = new List<TraineeStandingDto>();

			var sheets = await _unitOfWork.Sheets.Get()
							.Where(s => s.CampId == campId)
							.Select(s => new {
								s.Id,
								s.Name,
								Total = _onlineJudgeServices.GetContestStandingAsync(
								s.SheetCfId,
								1,
								true,
								s.IsSohag ? _codeForceConnection.SohagKey : _codeForceConnection.AssuitKey,
								s.IsSohag ? _codeForceConnection.SohagSecret : _codeForceConnection.AssuitSecret).Result
							}).ToListAsync();

			var Trainees = await _userManager.Users
						.Include(u => u.Trainee)
						.Where(u => u.Trainee != null && u.Trainee.CampId == campId)
						.Select(u => new { u.Trainee.Id, FullName = u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName })
			.ToListAsync();

			var traineeSheets = await _unitOfWork.TraineesSheetsAccess.Get()
								.Where(acc =>
									sheets.Select(i => i.Id).ToList()
									.Contains(acc.SheetId))
								.ToListAsync();

			foreach (var trainee in Trainees)
			{
				var traineeStanding = new TraineeStandingDto() { Name = trainee.FullName };

				foreach (var sheet in sheets)
				{
					traineeStanding.stand.Add(new SheetInfo()
					{
						Id = sheet.Id,
						Name = sheet.Name,
						Total = sheet.Total == null ? 26 : sheet.Total.result.problems.Count,
						Count = traineeSheets.FirstOrDefault(s => s.SheetId == sheet.Id && s.TraineeId == trainee.Id)?
										.NumberOfProblems ?? 0
					});
				}
				response.Add(traineeStanding);
			}

			return response;
		}
	}
}
