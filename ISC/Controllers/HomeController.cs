using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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
		public async Task<IActionResult> displayFeedbacks()
		{
			var Feedbacks =await _UnitOfWork.SessionsFeedbacks.getTopFeedbacksAsync(3);
			List<object> traineesFeeds = new List<object>();
			foreach(var feed in Feedbacks)
			{
				var Trainee = await _UnitOfWork.Trainees.getByIdAsync(feed.TraineeId);
				if (Trainee == null) continue;
				var Account = await _UserManager.FindByIdAsync(Trainee.UserId);
				if (Account == null) continue;
				string FullName=string.Concat(Account.FirstName,' ',Account.MiddleName);
				traineesFeeds.Add(new {FullName,feed.Feedback,feed.Rate });
			}
			if (traineesFeeds.Count == 0)
				return BadRequest("No feedbacks found!");

			return Ok(traineesFeeds);
		}
	}
}
