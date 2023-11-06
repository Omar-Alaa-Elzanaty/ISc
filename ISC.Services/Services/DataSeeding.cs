using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.EF.Repositories;
using ISC.Services.ISerivces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services
{
    public class DataSeeding:IDataSeeding
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DataBase _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DataSeeding(IUnitOfWork unitOfWork, UserManager<UserAccount> userManager, DataBase context, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

		public async Task Seeding()
		{
			try
			{
				if (_context.Database.GetPendingMigrations().Any())
				{
					_context.Database.Migrate();
				}
				else return;
			}
			catch
			{
				throw;
			}
			UserAccount admin = new UserAccount()
			{
				UserName = "ICPCAdmin",
				Email = "icpc.sohag.community@gmail.com",
				FirstName = "ICPC",
				MiddleName = "ICPC",
				LastName = "ICPC",
				College="FCI Sohag",
				BirthDate = DateTime.Now,
				PhoneNumber = "01123652462",
				NationalId = "1211111111111",
				Gender = "Male",
				Grade=4,
				CodeForceHandle= "IcpcSohag"
			};
			try
			{
				var result = await _userManager.CreateAsync(admin, "Admin321!567");
				if (result.Succeeded)
				{
					await _userManager.AddToRoleAsync(admin, "Admin");
				}
				else
				{
					throw new Exception("Invalid Startup");
				}
			}
			catch
			{
                Console.WriteLine("invalid");
            }
		}
	}
}
