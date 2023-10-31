using ISC.Core.APIDtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ISC.Core.APIDtos.CodeForcesDtos;

namespace ISC.Services.Services.ModelSerivces
{
    public class SheetServices : ISheetServices
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly CodeForceConnection _cfConnection;
		private readonly IOnlineJudgeServices _onlineJudgeServices;
		public SheetServices(IUnitOfWork unitOfWork, IOnlineJudgeServices onlineJudgeServices, IOptions<CodeForceConnection> cfConnection)
		{
			_unitOfWork = unitOfWork;
			_onlineJudgeServices = onlineJudgeServices;
			_cfConnection = cfConnection.Value;
		}

		public async Task<ServiceResponse<HashSet<int>>> TraineesFilter(List<TraineeSheetAccess> traineeAccess, Dictionary<int, int> ProblemSheetCount)
		{
			ServiceResponse<HashSet<int>>response= new ServiceResponse<HashSet<int>>();
			HashSet<int> filteredOnSheets = new HashSet<int>();
			int TSA_Size = traineeAccess.Count();
			for (int Trainee = 0; Trainee < TSA_Size; ++Trainee)
			{
				double precent = traineeAccess[Trainee].NumberOfProblems / (double)ProblemSheetCount[traineeAccess[Trainee].SheetId];
				int TraineePrecent = ((int)Math.Ceiling(precent * 100.0));
				if (TraineePrecent < traineeAccess[Trainee].Sheet.MinimumPrecent)
				{
					filteredOnSheets.Add(traineeAccess[Trainee].TraineeId);
					int CurrentTrainee = traineeAccess[Trainee].TraineeId;
					while (Trainee < TSA_Size && traineeAccess[Trainee].TraineeId == CurrentTrainee) ++Trainee;
					--Trainee;
				}
			}
			if (filteredOnSheets.Count == 0)
			{
				response.Success = false;
				response.Comment = "Filter sheet is empty";
				return response;
			}
			response.Success = true;
			response.Entity = filteredOnSheets;
			return response;
		}

		public async Task<ServiceResponse<List<TraineeSheetAccess>>> TraineeSheetAccesWithout(List<int>traineesId,int campId)
		{
			var response=new ServiceResponse<List<TraineeSheetAccess>>();
			bool[]? IsFound;
			if (traineesId.Count() > 0)
			{
				IsFound = new bool[traineesId.Max() + 1];
				int size = traineesId.Count();
				for (int i = 0; i < size; i++)
				{
					IsFound[traineesId[i]] = true;
				}
			}
			else IsFound = null;

			var traineeSheetAcess = _unitOfWork.TraineesSheetsAccess
				.findManyWithChildAsync(ts => ts.Sheet.CampId == campId && (IsFound != null ? !IsFound[ts.TraineeId] : true)
				, new[] { "Sheet", "Trainee" }).Result.OrderBy(ts => ts.TraineeId).ToList();
			if (traineeSheetAcess.Count == 0)
			{
				response.Success = false;
				response.Comment = "Couldn't found any Access for trainees";
				return response;
			}
			response.Success = true;
			response.Entity = traineeSheetAcess;
			return response;
		}
		public async Task<ServiceResponse<Dictionary<int, int>>> TraineeSheetProblemsCount(List<TraineeSheetAccess> traineesAcces)
		{
			ServiceResponse<Dictionary<int, int>> response = new ServiceResponse<Dictionary<int, int>>();
			Dictionary<int, int> ProblemSheetCount = traineesAcces
				.DistinctBy(tsa => tsa.SheetId)
				.Select(i => new {
					i.SheetId,
					Count = _onlineJudgeServices.GetContestStandingAsync(i.Sheet.SheetCfId, 1, true,
							i.Sheet.IsSohag ? _cfConnection.SohagKey : _cfConnection.AssuitKey
							, i.Sheet.IsSohag ? _cfConnection.SohagSecret : _cfConnection.AssuitSecret)
				.Result.result.problems.Count()
				})
				.ToDictionary(i => i.SheetId, i => i.Count);
			if (ProblemSheetCount.Count > 0)
			{
				response.Success = true;
				response.Entity = ProblemSheetCount;
				return response;
			}
			response.Success = false;
			response.Comment = "Coudn't find sheets or problems of sheets";
			return response;
		}
		public async Task<ServiceResponse<List<CodeforceSubmisionDto>>> SheetStatus(int contestId, bool isSohag)
		{
			var response = new ServiceResponse<List<CodeforceSubmisionDto>>();
			var SheetStatus = _onlineJudgeServices.GetContestStatusAsync(
				contestid:contestId.ToString(),
				apikey:isSohag ? _cfConnection.SohagKey : _cfConnection.AssuitKey,
				apisecret:isSohag ? _cfConnection.SohagSecret : _cfConnection.AssuitSecret,
				count:500).Result.result;
			if (SheetStatus == null)
			{
				response.Comment = $"Please Check from Contests Id's";
				return response;
			}
			response.Success = true;
			response.Entity = SheetStatus;
			return response;
		}
		public async Task<ServiceResponse<CodeforceStandingResultDto>>SheetStanding(int contestId, bool isSohag)
		{
			var response= new ServiceResponse<CodeforceStandingResultDto>();
			var standing = _onlineJudgeServices.GetContestStandingAsync(contestId.ToString(), 1, true,
				isSohag ? _cfConnection.SohagKey : _cfConnection.AssuitKey,
				isSohag ? _cfConnection.SohagSecret : _cfConnection.AssuitSecret)?.Result.result;

			if(standing == null)
			{
				response.Comment = "Cheek ContestId";
				return response;
			}

			response.Success = true;
			response.Entity = standing;
			return response;
		}
	}
}
