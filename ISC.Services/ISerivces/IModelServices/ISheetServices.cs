using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.ISerivces.IModelServices
{
    public interface ISheetServices
    {
        Task<ResponseModel<List<TraineeSheetAccess>>> TraineeSheetAccesWithout(List<int> traineesId, int campId);
        Task<ResponseModel<HashSet<int>>> TraineesFilter(List<TraineeSheetAccess> traineeAccess, Dictionary<int, int> ProblemSheetCount);
    }
}
