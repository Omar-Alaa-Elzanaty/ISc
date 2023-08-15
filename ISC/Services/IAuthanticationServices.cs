using ISC.API.APIDtos;
using ISC.Core.Models;

namespace ISC.API.Services
{
    public interface IAuthanticationServices
    {
        Task<AuthModel> adminRegisterAsync(AdminRegisterDto user);
        Task<AuthModel> loginAsync(LoginDto user);

    }
}
