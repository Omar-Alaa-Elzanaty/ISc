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
using System.Text;

namespace ISC.API.Services
{
    public class AuthanticationServices : IAuthanticationServices
	{
		private readonly UserManager<UserAccount> _UserManager;
		private readonly JWT _jwt;
		private readonly RoleManager<IdentityRole> _RoleManager;
		private readonly IUnitOfWork _unitOfWork;
        public AuthanticationServices(UserManager<UserAccount> usermanger,IOptions<JWT>jwt,IUnitOfWork unitofwork,RoleManager<IdentityRole>rolemanager)
        {
            _UserManager = usermanger;
			_jwt = jwt.Value;
			_unitOfWork = unitofwork;
			_RoleManager = rolemanager;
        }
		public async Task<AuthModel> adminRegisterAsync(AdminRegisterDto user)
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
			if (user.Roles.SingleOrDefault(Roles.TRAINEE) != null && (user.MentorId == null || user.CampId == null))
			{
				return new AuthModel()
				{
					Message = "Should assign mentor to spacific Mentor and Camp",
					IsAuthenticated = false
				};
			}
			UserAccount NewAccount = new UserAccount()
			{
				UserName = user.UserName,
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
				LastLoginDate=DateTime.Now,
			};
			var result = await _UserManager.CreateAsync(NewAccount, user.Password);
			if (result.Succeeded == false)
			{
				var errors = string.Empty;
				foreach(var error in result.Errors)
				{
					errors +=$"{error.Description},";
				}
				errors.Remove(errors.Length - 1, 1);
				return new AuthModel() { Message = errors};
			}
			await _UserManager.AddToRolesAsync(NewAccount, user.Roles);
			foreach (var Role in user.Roles)
			{
				if (Role == Roles.MENTOR)
				{
					Mentor Mentor = new Mentor() { UserId = NewAccount.Id };
					_unitOfWork.Mentors.addAsync(Mentor);
				}
				else if (Role == Roles.HOC)
				{
					if(user.CampId!=null)
					{
						HeadOfTraining HeadOfTraining = new HeadOfTraining() { UserId = NewAccount.Id, CampId = user.CampId };
						_unitOfWork.HeadofCamp.addAsync(HeadOfTraining);
					}
				}else if (Role == Roles.TRAINEE)
				{
					if (user.CampId != null && user.MentorId != null)
					{
						Trainee trainee = new Trainee() { UserId = NewAccount.Id, CampId = user.CampId, MentorId = user.MentorId };
						_unitOfWork.Trainees.addAsync(trainee);
					}
				}
			}
			await _unitOfWork.comleteAsync();
			var JwtSecurityToken = await CreateJwtToken(NewAccount);
			return new AuthModel()
			{
				ExpireOn = JwtSecurityToken.ValidTo,
				IsAuthenticated = true,
				Roles = user.Roles,
				Token = new JwtSecurityTokenHandler().WriteToken(JwtSecurityToken),
				UserId=NewAccount.Id
			};
		}

		public async Task<AuthModel> loginAsync(LoginDto user)
		{
			var UserAccount = await _UserManager.FindByNameAsync(user.UserName);
			if(user is null ||! await _UserManager.CheckPasswordAsync(UserAccount, user.Password))
			{
				return new AuthModel() { Message = "Email or Passwrod is incorrect!" };
			}
			JwtSecurityToken JwtSecurityToken;
			if (user.RemeberMe!=null)
			JwtSecurityToken = await CreateJwtToken(UserAccount,(bool)user.RemeberMe);
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
		public async Task<AuthModel> registerationValidation(AdminRegisterDto user)
		{
			if (await _UserManager.FindByNameAsync(user.UserName) != null)
				return new AuthModel() { Message = "Username is already Exist!" };
			if (await _UserManager.FindByEmailAsync(user.Email) != null)
				return new AuthModel() { Message = "Email is already registered!" };
			if (user.PhoneNumber != null && _UserManager.Users.SingleOrDefault(ac => ac.PhoneNumber == user.PhoneNumber) != null)
				return new AuthModel() { Message = "This number is already Exist!" };
			if (user.VjudgeHandle != null && _UserManager.Users.SingleOrDefault(i => i.VjudgeHandle == user.VjudgeHandle) != null)
				return new AuthModel() { Message = "Vjudge Handle is already Exist!" };
			if (_UserManager.Users.SingleOrDefault(i => i.CodeForceHandle == user.CodeForceHandle) != null)
				return new AuthModel() { Message = "Codeforce Handle is already Exist!" };
			if (user.Roles == null)
				return new AuthModel() { Message = "User should assign to one or more Roles" };
			return new AuthModel(){ IsAuthenticated = true};
		}
	}
}
