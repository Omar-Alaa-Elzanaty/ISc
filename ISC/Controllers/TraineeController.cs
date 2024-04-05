using System.Security.Claims;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.EF;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISC.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize(Roles = $"{Role.TRAINEE}")]
    public class TraineeController : ControllerBase
    {
        private readonly UserManager<UserAccount> _UserManager;
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ITraineeService _traineeSerivce;
        public TraineeController
            (
            UserManager<UserAccount> userManager,
            IAuthanticationServices auth,
            IUnitOfWork unitofwork,
            ITraineeService traineeSerivce)
        {
            _UserManager = userManager;
            _UnitOfWork = unitofwork;
            _traineeSerivce = traineeSerivce;
        }
        [HttpGet]
        public async Task<IActionResult> HaveComment()
        {
            ServiceResponse<int?> response = new ServiceResponse<int?>();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
            {
                return BadRequest(response);
            }

            var trainee = _UserManager.Users.Include(x => x.Trainee).FirstOrDefault(i => i.Id == userId);
            if (trainee is null || trainee.Trainee is null)
            {
                return BadRequest(false);
            }

            var lastSessionId = _UnitOfWork.Sessions
                                    .Get()
                                    .Where(s => s.Date < DateTime.Now)
                                    .OrderByDescending(x => x.Date)
                                    .FirstOrDefaultAsync()
                                    .Result?.Id;

            var attend = await _UnitOfWork.TraineesAttendence
                                    .Get()
                                    .AnyAsync(i => i.TraineeId == trainee.Trainee.Id);
            var hasComment = await _UnitOfWork.SessionsFeedbacks
                                    .Get()
                                    .AnyAsync(s => s.SessionId == lastSessionId && trainee.Trainee.Id == s.TraineeId);

            response.IsSuccess = attend == true && hasComment == false;
            response.Entity = lastSessionId;

            return Ok(response);
        }
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody]SessionFeedbackDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (userId is null)
            {
                return BadRequest("Invalid request");
            }

            return Ok(_traineeSerivce.AddFeedback(model, userId));
        }
        [HttpGet]
        public async Task<IActionResult> MentorInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Ok(await _traineeSerivce.MentorInfoAsync(userId));
        }
        [HttpGet]
        public async Task<IActionResult> TraineeCampName()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Ok(await _traineeSerivce.CampNameOfAsync(userId));
        }
        [HttpGet]
        public async Task<IActionResult> DisplaySheetsWithMaterials()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Ok(await _traineeSerivce.AccessSheetsWithMaterialsAsync(userId));
        }
        [HttpGet]
        public async Task<IActionResult> TraineeTasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Ok(await _traineeSerivce.GetTasks(userId));
        }
        [HttpPut]
        public async Task<IActionResult> UpdateTraineeTask(int taskId)
        {
            await _traineeSerivce.UpdateTaskState(taskId);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> DisplayTraineeInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if(userId is null)
            {
                return BadRequest("User not found");
            }

            return Ok(await _traineeSerivce.DisplayTraineeInfoAsync(userId));
        }
    }
}
