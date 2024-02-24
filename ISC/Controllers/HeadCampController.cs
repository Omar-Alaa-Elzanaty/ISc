using System.Security.Claims;
using AutoMapper;
using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using ISC.Core.ModelsDtos;
using ISC.EF;
using ISC.Services.ISerivces;
using ISC.Services.ISerivces.IModelServices;
using ISC.Services.Services.ExceptionSerivces.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISC.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize(Roles =$"{Roles.LEADER},{Roles.HOC}")]
    public class HeadCampController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailServices _MailService;
        private readonly IHeadSerivces _headServices;
        private readonly IMapper _mapper;
        public HeadCampController(UserManager<UserAccount> userManager,
            IUnitOfWork unitofwork,
            IMailServices mailService,
            IHeadSerivces headServices,
            IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitofwork;
            _MailService = mailService;
            _headServices = headServices;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> DisplayTrainees(string userId)
        {
            return Ok(await _headServices.DisplayTrainees(userId));
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteFromTrainees([FromBody] List<string> traineesUsersId)
        {
            return Ok(await _headServices.DeleteFromTrianee(traineesUsersId));
        }
        [HttpGet]
        public async Task<IActionResult> WeeklyFilter([FromBody] List<string> selectedTrainee)
        {
            var headOfCampUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (headOfCampUserId == null)
            {
                throw new BadRequestException("Invalid request");
            }

            return Ok(await _headServices.WeeklyFilterAsync(selectedTrainee, headOfCampUserId));
        }
        [HttpDelete]
        public async Task<IActionResult> SubmitWeeklyFilter([FromBody] List<string> traineesUsersId, string headUserId)
        {
            return Ok(await _headServices.SubmitWeeklyFilterAsync(traineesUsersId, headUserId));
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> DisplayMentors(string userId)
        {
            return Ok(await _headServices.DisplayMentorsAsync(userId));
        }
        [HttpGet]
        public async Task<IActionResult> DisplayMentorTrainee()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest("Invalid Request");
            }

            return Ok(await _headServices.DisplayTraineeMentorAsync(userId));
        }
        [HttpPost]
        public async Task<IActionResult> SubmitTraineesMentors([FromBody] List<AssignTraineeMentorDto> data)
        {
            return Ok(await _headServices.SubmitTraineeMentorAsync(data));
        }
        [HttpGet]
        public async Task<IActionResult> DisplaySessions(string userId)
        {
            return Ok(await _headServices.DisplaySessionsAsync(userId));
        }
        [HttpPost]
        public async Task<IActionResult> AddSession([FromBody] SessionDto model, string userId)
        {
            return Ok(await _headServices.AddSessionAsync(model, userId));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            return Ok(await _headServices.DeleteSessionAsync(id));
        }
        [HttpGet]
        public async Task<IActionResult> DisplayInstructor()
        {
            var response = new ServiceResponse<List<string>>()
            {
                IsSuccess = true,
                Entity = _userManager.GetUsersInRoleAsync(Role.INSTRUCTOR).Result.Select(i => i.FirstName + ' ' + i.MiddleName)
                        .ToList()
            };

            await Task.CompletedTask;
            return Ok(response);
        }
        //TODO: start from here
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSessionInfo([FromBody] SessionDto model, int id)
        {
            return Ok(await _headServices.UpdateSessionInfoAsync(model, id));
        }
        [HttpGet]
        public async Task<IActionResult> DisplaySheet(string userId)
        {
            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;
            if (campId is null)
            {
                return NotFound("Invalid account");
            }
            var sheets = _unitOfWork.Sheets
                .Get()
                .Where(s => s.CampId == campId)
                .OrderBy(s => s.SheetOrder)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.SheetLink,
                    s.SheetCfId,
                    s.IsSohag,
                    s.MinimumPrecent,
                    s.SheetOrder,
                });
            return Ok(sheets);
        }
        [HttpPost]
        public async Task<IActionResult> AddSheet([FromBody] SheetDto model)
        {
            var userId = "5f00d005-aafb-4bd9-8341-68a4cf2f8a22";// User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;

            model.Name = model.Name.ToLower();

            var isValidToAdd = await _unitOfWork.Sheets.isValidToAddAsync(model, campId);

            if (!isValidToAdd)
            {
                return BadRequest("Conflict found");
            }

            var sheet = _mapper.Map<Sheet>(model);
            sheet.CampId = (int)campId;

            await _unitOfWork.Sheets.AddAsync(sheet);
            _ = await _unitOfWork.completeAsync();

            return Ok("Success");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveSheet(int id)
        {
            var Sheet = await _unitOfWork.Sheets.GetByIdAsync(id);

            if (Sheet is null)
            {
                return NotFound("Couldn't delete");
            }

            if (!await _unitOfWork.Sheets.DeleteAsync(Sheet))
            {
                return BadRequest("Couldn't delete");
            }

            _unitOfWork.completeAsync().Wait();

            return Ok();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSheets(int id, [FromBody] SheetDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;

            var sheet = await _unitOfWork.Sheets.GetByIdAsync(id);

            if (sheet is null || !await _unitOfWork.Sheets.isValidToUpdateAsync(model, campId, id))
            {
                return NotFound("Couldn't update");
            }
            sheet.Name = model.Name;
            sheet.SheetLink = model.SheetLink;
            sheet.IsSohag = model.IsSohag;
            sheet.SheetCfId = model.SheetCfId;
            sheet.MinimumPrecent = model.MinimumPrecent;
            sheet.SheetOrder = model.SheetOrder;

            await _unitOfWork.Sheets.UpdateAsync(sheet);
            _ = await _unitOfWork.completeAsync();

            return Ok("Success");
        }
        [HttpGet("{sheetId}")]
        public async Task<IActionResult> DisplayMaterials(int sheetId)
        {
            var materials = await _unitOfWork.Materials.FindManyWithChildAsync(m => m.SheetId.Equals(sheetId));

            return Ok(materials.OrderBy(m => m.CreationDate).Select(m => new
            {
                m.Id,
                m.Name,
                m.Link
            }).ToList());
        }
        [HttpPost]
        public async Task<IActionResult> AddMaterial(int sheetId, MaterialDto model)
        {
            if (await _unitOfWork.Sheets.GetByIdAsync(sheetId) == null)
            {
                return BadRequest("Invalid request");
            }

            if (await _unitOfWork.Materials.Get().AnyAsync(m => (m.Name == model.Name || m.Link == model.Link) && m.SheetId == sheetId))
            {
                return BadRequest("Data conflict");
            }

            Material material = _mapper.Map<Material>(model);
            material.SheetId = sheetId;

            await _unitOfWork.Materials.AddAsync(material);
            _ = await _unitOfWork.completeAsync();

            return Ok("Success");
        }
        [HttpDelete("{materialId}")]
        public async Task<IActionResult> RemoveMaterial(int materialId)
        {
            var material = await _unitOfWork.Materials.GetByIdAsync(materialId);

            if (material == null)
            {
                return NotFound("Invalid Request");
            }

            await _unitOfWork.Materials.DeleteAsync(material);
            _ = await _unitOfWork.completeAsync();

            return Ok();
        }
        [HttpPut("{materialId}")]
        public async Task<IActionResult> UpdateMaterial(int materialId, MaterialDto model)
        {
            var material = await _unitOfWork.Materials.GetByIdAsync(materialId);

            var isValidToUpdate = !await _unitOfWork.Materials.Get()
                                        .AnyAsync(m => (m.Name == model.Name || m.Link == model.Link) && m.SheetId != material.SheetId);
            if (!isValidToUpdate)
            {
                return BadRequest("Conflict Update");
            }

            material.Name = model.Name;
            material.Link = model.Link;

            await _unitOfWork.Materials.UpdateAsync(material);
            _ = await _unitOfWork.completeAsync();

            return Ok("Success");
        }
        [HttpGet]
        public async Task<IActionResult> DisplaySheetAccess()
        {
            var userId = "5f00d005-aafb-4bd9-8341-68a4cf2f8a22";// User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var sheets = _unitOfWork.HeadofCamp.Get().Include(h => h.Camp)?.Include(h => h.Camp.Sheets)
                .FirstOrDefault(h => h.Camp != null && h.UserId == userId)?.Camp?.Sheets.Select(i => new
                {
                    i.Id,
                    i.Name
                }).ToList();

            var sheetAccess = _unitOfWork.TraineesSheetsAccess.FindManyWithChildAsync(ac => sheets.Any(s => s.Id == ac.SheetId))
                                .Result.Select(s => s.SheetId).ToList();

            var access = new List<SheetAccessStatusDto>();

            foreach (var sheet in sheets)
            {
                var sheetStatus = new SheetAccessStatusDto()
                {
                    SheetId = sheet.Id,
                    Name = sheet.Name,
                    IsReachAble = sheetAccess.Contains(sheet.Id)
                };
                access.Add(sheetStatus);
            }

            return Ok(access);
        }
        [HttpGet]
        public async Task<IActionResult> DisplayTraineeSheetAccess()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("Invalid request");
            }
            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;

            return Ok(await _headServices.DisplayTraineeAccess(campId ?? 0));

        }
        [HttpPost("{sheetId}")]
        public async Task<IActionResult> AddNewAccess(int sheetId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest("Invalid request");
            }
            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;

            await _headServices.AddNewTrainingSheetAccess(sheetId, campId ?? 0);

            return Ok("Success");
        }
        [HttpPut("{sheetId}/{traineeId}")]
        public async Task<IActionResult> UpdateTraineeAccess(int sheetId, int traineeId)
        {
            await _unitOfWork.TraineesSheetsAccess.AddAsync(new TraineeSheetAccess() { TraineeId = traineeId, SheetId = sheetId });
            _ = await _unitOfWork.completeAsync();
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> AttendenceAccessPage()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest("Invalid request");
            }
            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;

            var availableMentors = await _userManager.Users.Include(u => u.Mentor)
                                    .Where(u => u.Mentor != null).Select(u => new
                                    {
                                        u.Mentor.Id,
                                        FullName = u.FirstName + ' ' + u.MiddleName + ' ' + u.LastName,
                                    }).ToListAsync();

            var sessions = await _unitOfWork.Sessions.Get()
                            .Where(s => s.CampId == campId)
                            .Select(s => new
                            {
                                s.Id,
                                s.Topic
                            }).ToListAsync();

            return Ok(new { availableMentors, sessions });
        }
        [HttpPut]
        public async Task<IActionResult> GiveAttendenceAccess(int mentorId, int sessionId)
        {
            var mentor = await _unitOfWork.Mentors.GetByIdAsync(mentorId);
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);

            if (mentor is null || session is null)
            {
                return BadRequest("Invalid request");
            }

            mentor.AccessSessionId = sessionId;

            _ = await _unitOfWork.completeAsync();

            return Ok("Success");
        }
        [HttpGet]
        public async Task<IActionResult> GeneralStanding()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest("Invalid request");
            }
            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;
            return Ok(await _headServices.GeneralStandingsAsync(campId));
        }
        [HttpGet]
        public async Task<IActionResult> MentorAttendence()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest("Invalid request");
            }

            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;

            return Ok(await _headServices.MentorAttendence(campId));
        }
        [HttpGet]
        public async Task<IActionResult> TraineeAttendence()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest("Invalid request");
            }

            var campId = _unitOfWork.HeadofCamp.GetByUserIdAsync(userId).Result?.CampId;


            return Ok(await _headServices.TraineeAttendence(campId));
        }
    }
}
