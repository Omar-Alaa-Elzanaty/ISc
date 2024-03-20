using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISC.Core.Dtos;
using ISC.Core.Models;

namespace ISC.Services.ISerivces.IModelServices
{
    public interface IMentorServices
    {
        Task<ServiceResponse<List<DisplayCampsForMentorDto>>> GetCampsNames(string userId);
    }
}
