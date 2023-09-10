using ISC.API.APIDtos;
using ISC.Core.Models;

namespace ISC.API.ISerivces
{
    public interface IAuthanticationServices
    {
        Task<AuthModel> RegisterAsync(RegisterDto user);
        Task<AuthModel> loginAsync(LoginDto user);

    }
}
