using ISC.Core.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF
{
	public class UserAccount:IdentityUser
	{
		// username,password,id,email,phonenumber
		public string FirstName { get; set; } = "NOName";
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		[RegularExpression("^[1-9][0-9]*$")]
		public string NationalId { get; set; }
		public DateTime BirthDate { get; set; }
		public int Grade { get; set; }
		public string College { get; set; }
		public DateTime JoinDate { get; set; }= DateTime.Now;
		public string Gender { get; set; }
		public DateTime LastLoginDate { get; set; }=DateTime.Now;
		public byte[]? ProfilePicture { get; set; }
		public string CodeForceHandle { get; set; }
		public string? FacebookLink { get; set; }
		public string? VjudgeHandle { get; set; }
		public Mentor? Mentor { get; set; }
		public Trainee? Trainee { get; set; }
		public HeadOfTraining? Headofcamp { get; set; }
		public string generateUserName()
		{
			StringBuilder UserName=new StringBuilder("ICPC");
			var rand=new Random();
			int x = rand.Next(10, 100);
			while (x > 0)
			{
				UserName.Append(this.NationalId[x%10]);
				x /= 10;
			}
			x = 3;
			while (x-- > 0)
			{
				UserName.Append(this.CodeForceHandle[rand.Next(this.CodeForceHandle.Length-1)]);
			}
			x = 2;
			while (x-- > 0)
			{
				UserName.Append(this.NationalId[rand.Next(12)]);
			}
			return UserName.ToString();
		}
		public string generatePassword()
		{
			StringBuilder NewPassword = new StringBuilder("ICPC"),
						  HashingSohag=new StringBuilder("sohag"),
						  HashingSymbols = new StringBuilder("!@#$&");
			var rand= new Random();
			int x = 3;
			while(x > 0){
				NewPassword.Append(HashingSohag[rand.Next(HashingSohag.Length-1)]);
				x--;
			}
			NewPassword.Append(HashingSymbols[rand.Next(HashingSymbols.Length-1)]);
			x = rand.Next(100, 1001);
			while(x > 0)
			{
				NewPassword.Append(x%10+'0');
				x /= 10;
			}
			return NewPassword.ToString();
		}
	}
}
