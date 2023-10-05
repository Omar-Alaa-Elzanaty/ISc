using ISC.Core.APIDtos;
using ISC.Core.Models;

namespace ISC.Services.ISerivces
{
    public interface IAuthanticationServices
    {
        Task<AuthModel> RegisterAsync(RegisterDto user);
        Task<AuthModel> loginAsync(LoginDto user);

    }
}
