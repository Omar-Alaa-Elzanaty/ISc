using ISC.Core.Models;
using ISC.Services.APIDtos;

namespace ISC.Services.ISerivces
{
    public interface IAuthanticationServices
    {
        Task<AuthModel> RegisterAsync(RegisterDto user);
        Task<AuthModel> loginAsync(LoginDto user);

    }
}
