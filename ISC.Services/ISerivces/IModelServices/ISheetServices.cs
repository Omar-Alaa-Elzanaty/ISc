using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ISC.Core.APIDtos.CodeForcesDtos;

namespace ISC.Services.ISerivces.IModelServices
{
    public interface ISheetServices
    {
        Task<ServiceResponse<List<TraineeSheetAccess>>> TraineeSheetAccesWithout(List<int> traineesId, int campId);
        Task<ServiceResponse<HashSet<int>>> TraineesFilter(List<TraineeSheetAccess> traineeAccess, Dictionary<int, int> ProblemSheetCount);
		Task<ServiceResponse<Dictionary<int, int>>> TraineeSheetProblemsCount(List<TraineeSheetAccess> traineesAcces);
        Task<ServiceResponse<List<CodeforceSubmisionDto>>> SheetStatus(int contestId, bool isSohag);
        Task<ServiceResponse<CodeforceStandingResultDto>> SheetStanding(int contestId, bool isSohag);
	}
}
