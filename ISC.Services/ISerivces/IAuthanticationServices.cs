using ISC.Core.APIDtos;
using ISC.Core.Models;

namespace ISC.Services.ISerivces
{
    public interface IAuthanticationServices
    {
        Task<AuthModel> RegisterAsync(RegisterDto user,string? message= null, bool sendEmail = false);
        Task<AuthModel> loginAsync(LoginDto user);

    }
}
