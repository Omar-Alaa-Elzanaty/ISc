using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection.Metadata.Ecma335;

namespace ISC.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HomeController : ControllerBase
	{
		private readonly IUnitOfWork _UnitOfWork;
		private readonly UserManager<UserAccount> _UserManager;
		public HomeController(IUnitOfWork unitOfWork,UserManager<UserAccount>usermanager)
        {
            _UnitOfWork = unitOfWork;
			_UserManager = usermanager;
        }

		[HttpGet("DisplayFeedbacks")]
		public async Task<IActionResult> displayFeedbacksAsync()
		{
			var Feedbacks =await _UnitOfWork.SessionsFeedbacks.getTopRateAsync(3);
			if(Feedbacks.Count == 0 )
			{
				return BadRequest("NO feedbacks found!");
			}
			var Trainees = await _UnitOfWork.Trainees.getAllAsync(tr=>Feedbacks.Exists(i => i.TraineeId == tr.Id));
			var Result = (from Trainee in Trainees
						 join user in _UserManager.Users
						 on Trainee.UserId equals user.Id
						 join Feed in Feedbacks
						 on Trainee.Id equals Feed.TraineeId
						 select new { FullName = user.FirstName + ' ' + user.MiddleName + ' ' + user.LastName,
  						 Feed.Feedback,Feed.Rate }).ToList();

			if (Result.Count == 0)
				return BadRequest("No feedbacks found!");

			return Ok(Result);
		}
	}
}
