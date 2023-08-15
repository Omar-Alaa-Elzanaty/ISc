using ISC.API.APIDtos;
using ISC.API.Helpers;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ISC.API.Services
{
    public class AuthanticationModel : IAuthanticationServices
	{
		private readonly UserManager<UserAccount> _userManager;
		private readonly JWT _jwt;
		private readonly IUnitOfWork _unitOfWork;
        public AuthanticationModel(UserManager<UserAccount> usermanger,IOptions<JWT>jwt,IUnitOfWork unitofwork)
        {
            _userManager = usermanger;
			_jwt = jwt.Value;
			_unitOfWork = unitofwork;
        }
		public async Task<AuthModel> adminRegisterAsync(AdminRegisterDto user)
		{
			AuthModel NotValidData =await registerationValidation(user);
			if (NotValidData.IsAuthenticated==false)
			{
				return  NotValidData;
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
			var result = await _userManager.CreateAsync(NewAccount, user.Password);
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
			await _userManager.AddToRolesAsync(NewAccount, user.Roles);
			foreach (var Role in user.Roles)
			{
				if (Role == Roles.MENTOR)
				{
					Mentor Mentor = new Mentor() { UserId = NewAccount.Id };
					_unitOfWork.Mentors.addAsync(Mentor);
				}
				else if (Role == Roles.HOC)
				{
					HeadOfTraining HeadOfTraining = new HeadOfTraining() { UserId = NewAccount.Id,CampId=user.CampId };
					_unitOfWork.HeadofCamp.addAsync(HeadOfTraining);
				}else if (Role == Roles.TRAINEE)
				{
					Trainee trainee = new Trainee() { UserId = NewAccount.Id, CampId = (int)user.CampId, MentorId = (int)user.MentorId };
					_unitOfWork.Trainees.addAsync(trainee);
				}
			}
			_unitOfWork.comlete();
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
			var UserAccount = await _userManager.FindByNameAsync(user.UserName);
			if(user is null ||! await _userManager.CheckPasswordAsync(UserAccount, user.Password))
			{
				return new AuthModel() { Message = "Email or Passwrod is incorrect!" };
			}
			var JwtSecurityToken = await CreateJwtToken(UserAccount);
			var RolesList=await _userManager.GetRolesAsync(UserAccount);
			return new AuthModel()
			{
				IsAuthenticated = true,
				Token = new JwtSecurityTokenHandler().WriteToken(JwtSecurityToken),
				UserId = UserAccount.Id,
				ExpireOn = JwtSecurityToken.ValidTo,
				Roles = RolesList.ToList()
			};
		}

		private async Task<JwtSecurityToken> CreateJwtToken(UserAccount user)
		{
			var userClaims = await _userManager.GetClaimsAsync(user);
			var roles = await _userManager.GetRolesAsync(user);
			var roleClaims = new List<Claim>();

			foreach (var role in roles)
				roleClaims.Add(new Claim("roles", role));

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
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
				expires: DateTime.UtcNow.AddDays(_jwt.DurationInDays),
				signingCredentials: signingCredentials);

			return jwtSecurityToken;
		}
		public async Task<AuthModel> registerationValidation(AdminRegisterDto user)
		{
			if (await _userManager.FindByNameAsync(user.UserName) != null)
				return new AuthModel() { Message = "Username is already Exist!" };
			if (await _userManager.FindByEmailAsync(user.Email) != null)
				return new AuthModel() { Message = "Email is already registered!" };
			if (user.PhoneNumber != null && _userManager.Users.SingleOrDefault(ac => ac.PhoneNumber == user.PhoneNumber) != null)
				return new AuthModel() { Message = "This number is already Exist!" };
			if (user.VjudgeHandle != null && _userManager.Users.SingleOrDefault(i => i.VjudgeHandle == user.VjudgeHandle) != null)
				return new AuthModel() { Message = "Vjudge Handle is already Exist!" };
			if (_userManager.Users.SingleOrDefault(i => i.CodeForceHandle == user.CodeForceHandle) != null)
				return new AuthModel() { Message = "Codeforce Handle is already Exist!" };
			if (user.Roles == null)
				return new AuthModel() { Message = "User should assign to one or more Roles" };
			return new AuthModel(){ IsAuthenticated = true};
		}
	}
}
