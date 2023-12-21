using ISC.Core.APIDtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.Helpers;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace ISC.Services.Services
{
    public class AuthanticationServices : IAuthanticationServices
	{
		private readonly UserManager<UserAccount> _UserManager;
		private readonly JWT _jwt;
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly IUnitOfWork _UnitOfWork;
		private readonly DefaultMessages _defaultMessages;
		private readonly IOnlineJudgeServices _Onlinejudge;
		private readonly IMailServices _MailServices;
		private readonly IMediaServices _MediaServices;
		public AuthanticationServices(
			UserManager<UserAccount> usermanger,
			IOptions<JWT> jwt,
			IUnitOfWork unitofwork,
			RoleManager<IdentityRole> rolemanager,
			IOnlineJudgeServices onlineJudge,
			IMailServices mailservices,
			IOptions<DefaultMessages> messages
,
			IMediaServices mediaServices)
		{
			_UserManager = usermanger;
			_jwt = jwt.Value;
			_UnitOfWork = unitofwork;
			_RoleManager = rolemanager;
			_Onlinejudge = onlineJudge;
			_MailServices = mailservices;
			_defaultMessages = messages.Value;
			_MediaServices = mediaServices;
		}
		public async Task<AuthModel> RegisterAsync(RegisterDto user,string? message=null,bool sendEmail=false)
		{
			AuthModel NotValidData =await RegisterationValidation(user);
			if (NotValidData.IsAuthenticated==false)
			{
				return  NotValidData;
			}
			if (!await _RoleManager.RoleExistsAsync(user.Role))
			{
				return new AuthModel()
				{
					Message = $"Role {user.Role} not exist in system",
					IsAuthenticated = false
				};
			}
			if (user.Role.Contains(Role.TRAINEE) == true && (user.CampId == null))
			{
				return new AuthModel()
				{
					Message = "Should assign Trainee to spacific Camp",
					IsAuthenticated = false
				};
			}
			var newAccount = new UserAccount()
			{
				Email = user.Email,
				PhoneNumber = user.PhoneNumber,
				FirstName = user.FirstName,
				MiddleName = user.LastName,
				LastName = user.LastName,
				NationalId = user.NationalId,
				BirthDate = Convert.ToDateTime(user.BirthDate),
				Grade = user.Grade,
				College = user.College,
				Gender = user.Gender,
				CodeForceHandle = user.CodeForceHandle,
				FacebookLink = user.FacebookLink,
				VjudgeHandle = user.VjudgeHandle,
				PhotoUrl=user.ProfilePicture != null? await _MediaServices.AddAsync(user.ProfilePicture)??string.Empty: null,
				JoinDate=DateTime.Now,
				LastLoginDate=DateTime.Now
			};
			var password = newAccount.GeneratePassword();
			newAccount.UserName = newAccount.GenerateUserName();
			
			var result = await _UserManager.CreateAsync(newAccount, password);
			if (result.Succeeded == false)
			{
				var errors = string.Empty;
				foreach(var error in result.Errors)
				{
					errors +=$"{error.Description},";
				}
				errors.Remove(errors.Length - 1, 1);
				await _UserManager.DeleteAsync(newAccount);
				return new AuthModel() { Message = errors};
			}

			bool Result = await _UnitOfWork.addToRoleAsync(newAccount, user.Role, user.CampId, user.MentorId);
			
			if (Result == false)
			{
				await _UserManager.DeleteAsync(newAccount);
				return new AuthModel()
				{
					Message = $"May be some of roles must be add or modify on Role {user.Role}... please try again.",
					IsAuthenticated = false
				};
			}
			//TODO: remove comment
			//if (!sendEmail)
			//{
			//	bool EmailResult = await _MailServices.sendEmailAsync(
			//	user.Email,
			//	"ICPC Sohag",
			//	(message == null ? _defaultMessages.DefaultRegisterMessage
			//	: message.Replace("FN",newAccount.FirstName)
			//			.Replace("LN",newAccount.MiddleName)
			//			.Replace("CAMP",_UnitOfWork.Camps.getByIdAsync(Convert.ToInt32(user.CampId)).Result.Name)
			//					.Replace("USERNAME", newAccount.UserName).Replace("PASSWORD", password))
			//	);
			//	if (EmailResult == false)
			//	{
			//		await _UserManager.DeleteAsync(newAccount);
			//		return new AuthModel() { Message = "Email is not Valid" };
			//	}
			//}
			await _UnitOfWork.completeAsync();
			var JwtSecurityToken = await CreateJwtToken(newAccount);
			return new AuthModel()
			{
				ExpireOn = JwtSecurityToken.ValidTo,
				IsAuthenticated = true,
				Token = new JwtSecurityTokenHandler().WriteToken(JwtSecurityToken),
				UserId = newAccount.Id,
				UserName = newAccount.UserName,
				Password = password
			};
		}

		public async Task<AuthModel> loginAsync(LoginDto user)
		{
			var UserAccount = await _UserManager.FindByNameAsync(user.UserName);
			if (UserAccount is null || !await _UserManager.CheckPasswordAsync(UserAccount, user.Password)) 
			{
				return new AuthModel() { Message = "Email or Passwrod is inCorrect!" };
			}

			var RolesList=await _UserManager.GetRolesAsync(UserAccount);
			UserAccount.LastLoginDate = DateTime.Now;
			await _UserManager.UpdateAsync(UserAccount);

			JwtSecurityToken JwtSecurityToken = await CreateJwtToken(UserAccount, user.RememberMe ?? false);

			return new AuthModel()
			{
				IsAuthenticated = true,
				Token = new JwtSecurityTokenHandler().WriteToken(JwtSecurityToken),
				UserId = UserAccount.Id,
				ExpireOn = JwtSecurityToken.ValidTo,
				Roles = RolesList.ToList()
			};
		}

		private async Task<JwtSecurityToken> CreateJwtToken(UserAccount user,bool rememberme=false)
		{
			var userClaims = await _UserManager.GetClaimsAsync(user);
			var roles = await _UserManager.GetRolesAsync(user);
			var roleClaims = new List<Claim>();

			foreach (var role in roles)
				roleClaims.Add(new Claim("roles", role));

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim("uid", user.Id),
				new Claim("username",user.UserName)
			}
			.Union(userClaims)
			.Union(roleClaims);

			var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
			var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

			var jwtSecurityToken = new JwtSecurityToken(
				issuer: _jwt.Issuer,
				audience: _jwt.Audience,
				claims: claims,
				expires: (rememberme==true)?DateTime.UtcNow.AddDays(_jwt.DurationInDays):DateTime.UtcNow.AddDays(1),
				signingCredentials: signingCredentials);

			return jwtSecurityToken;
		}
		private async Task<AuthModel> RegisterationValidation(RegisterDto user)
		{
			if (await _UserManager.FindByEmailAsync(user.Email) != null)
				return new AuthModel() { Message = "Email is already registered!" };
			if (user.PhoneNumber != null && _UserManager.Users.SingleOrDefault(ac => ac.PhoneNumber == user.PhoneNumber) != null)
				return new AuthModel() { Message = "This number is already Exist!" };
			if (user.VjudgeHandle != null && _UserManager.Users.SingleOrDefault(i => i.VjudgeHandle == user.VjudgeHandle) != null)
				return new AuthModel() { Message = "Vjudge Handle is already Exist!" };
			//if (await _Onlinejudge.checkHandleValidationAsync(user.CodeForceHandle) == false)
			//	return new AuthModel() { Message = "Codeforce Handle is not valid!" };
			if (_UserManager.Users.SingleOrDefault(i => i.CodeForceHandle == user.CodeForceHandle) != null)
				return new AuthModel() { Message = "Codeforce Handle is already Exist!" };
			if (_UserManager.Users.SingleOrDefault(i => i.NationalId == user.NationalId) != null)
				return new AuthModel() { Message = "National Id is already exist!" };
			if (user.Role == null)
				return new AuthModel() { Message = "User should assign to one or more Roles" };
			return new AuthModel(){ IsAuthenticated = true};
		}
	}
}
