using ISC.API.APIDtos;
using ISC.API.Helpers;
using ISC.API.ISerivces;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace ISC.API.Services
{
    public class AuthanticationServices : IAuthanticationServices
	{
		private readonly UserManager<UserAccount> _UserManager;
		private readonly JWT _jwt;
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly IUnitOfWork _UnitOfWork;
		private readonly IOnlineJudgeServices _Onlinejudge;
		private readonly IMailServices _MailServices;
        public AuthanticationServices(UserManager<UserAccount> usermanger,IOptions<JWT>jwt,IUnitOfWork unitofwork,RoleManager<IdentityRole>rolemanager,IOnlineJudgeServices onlineJudge,IMailServices mailservices)
        {
            _UserManager = usermanger;
			_jwt = jwt.Value;
			_UnitOfWork = unitofwork;
			_RoleManager = rolemanager;
			_Onlinejudge = onlineJudge;
			_MailServices = mailservices;
        }
		public async Task<AuthModel> RegisterAsync(RegisterDto user)
		{
			AuthModel NotValidData =await registerationValidation(user);
			if (NotValidData.IsAuthenticated==false)
			{
				return  NotValidData;
			}
			foreach (var role in user.Roles)
			{
				if (!await _RoleManager.RoleExistsAsync(role))
				{
					return new AuthModel()
					{
						Message = $"Role {role} not exist in system",
						IsAuthenticated = false
					};
				}
			}
			if (user.Roles.Contains(Roles.TRAINEE) == true && (user.CampId == null))
			{
				return new AuthModel()
				{
					Message = "Should assign Trainee to spacific Camp",
					IsAuthenticated = false
				};
			}
			UserAccount NewAccount = new UserAccount()
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
				ProfilePicture = await new Converters().photoconverterasync(user.ProfilePicture),
				JoinDate=DateTime.Now,
				LastLoginDate=DateTime.Now
			};
			var Password = NewAccount.generatePassword();
			NewAccount.UserName = NewAccount.generateUserName();
			var result = await _UserManager.CreateAsync(NewAccount, Password);
			if (result.Succeeded == false)
			{
				var errors = string.Empty;
				foreach(var error in result.Errors)
				{
					errors +=$"{error.Description},";
				}
				errors.Remove(errors.Length - 1, 1);
				await _UserManager.DeleteAsync(NewAccount);
				return new AuthModel() { Message = errors};
			}
			foreach (var Role in user.Roles)
			{
				bool Result = await _UnitOfWork.addToRoleAsync(NewAccount, Role, user.CampId, user.MentorId);
				if (Result == false) 
				{
					await _UserManager.DeleteAsync(NewAccount);
					return new AuthModel()
					{
						Message = $"May be some of roles must be add or modify on Role {Role}... please try again.",
						IsAuthenticated = false
					};
				}
			}
			//string body = "We need to inform you that your account on ISc being ready to use\n" +
			//			"this is your creadntial informations\n" +
			//			$"Username: {NewAccount.UserName}\n" +
			//			$"\nPassword: {Password}";
			//bool EmailResult = await _MailServices.sendEmailAsync(user.Email, "ICPC Sohag account", body);
			//if (EmailResult == false)
			//{
			//	await _UserManager.DeleteAsync(NewAccount);
			//	return new AuthModel() { Message="Email is not Valid" };
			//}
			await _UnitOfWork.comleteAsync();
			var JwtSecurityToken = await CreateJwtToken(NewAccount);
			return new AuthModel()
			{
				ExpireOn = JwtSecurityToken.ValidTo,
				IsAuthenticated = true,
				Roles = user.Roles,
				Token = new JwtSecurityTokenHandler().WriteToken(JwtSecurityToken),
				UserId = NewAccount.Id,
				UserName = NewAccount.UserName,
				Password = Password
			};
		}

		public async Task<AuthModel> loginAsync(LoginDto user)
		{
			var UserAccount = await _UserManager.FindByNameAsync(user.UserName);
			if(UserAccount is null ||! await _UserManager.CheckPasswordAsync(UserAccount, user.Password))
			{
				return new AuthModel() { Message = "Email or Passwrod is incorrect!" };
			}
			UserAccount.LastLoginDate = DateTime.Now;
			await _UnitOfWork.comleteAsync();
			JwtSecurityToken JwtSecurityToken;
			if (user.RememberMe!=null)
			JwtSecurityToken = await CreateJwtToken(UserAccount,(bool)user.RememberMe);
			else
				JwtSecurityToken = await CreateJwtToken(UserAccount);
			var RolesList=await _UserManager.GetRolesAsync(UserAccount);
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
		private async Task<AuthModel> registerationValidation(RegisterDto user)
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
			if (user.Roles == null)
				return new AuthModel() { Message = "User should assign to one or more Roles" };
			return new AuthModel(){ IsAuthenticated = true};
		}
	}
}
