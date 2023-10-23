using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.Services.ISerivces.IModelServices;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services.ModelSerivces
{
    public class SheetServices : ISheetServices
	{
		private readonly IUnitOfWork _unitOfWork;
        public SheetServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork=unitOfWork;
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
	}
}
