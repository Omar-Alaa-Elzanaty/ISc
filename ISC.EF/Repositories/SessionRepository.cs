using ISC.Core.Dtos;
using ISC.Core.Interfaces;
using ISC.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	public class SessionRepository:BaseRepository<Session>, ISessionRepository
	{
        private readonly DataBase _Context;
		public SessionRepository(DataBase context) : base(context)
		{
			_Context = context;
		}
		public async Task<ServiceResponse<bool>>CheckUpdateAbility(Session oldsession,SessionDto newSession,int id)
		{
			var response = new ServiceResponse<bool>() { Success = true };

			if (newSession.InstructorName != oldsession.InstructorName)
			{
				response.Success = !await _Context.Sessions.AnyAsync(s =>
				s.InstructorName == newSession.InstructorName
				&& s.Id != id
				&& s.Date.Month == newSession.Date.Month && s.Date.Day == newSession.Date.Day);

				response.Comment += response.Success == false ? "Conflict in instructor\n" : string.Empty;
			}

			if (response.Success && oldsession.Topic != newSession.Topic)
			{
				response.Success = !await _Context.Sessions.AnyAsync(s =>
					s.Topic == newSession.Topic
					&& s.CampId == newSession.CampId
					&& s.Id != id
					&& s.Date.AddMonths(3) >= newSession.Date);

				response.Comment += response.Success == false ? "Conflict in topic name\n" : string.Empty;
			}

			if (response.Success && (oldsession.Date.Day != newSession.Date.Day || oldsession.Date.Month != newSession.Date.Month))
			{
				response.Success = !await _Context.Sessions.AnyAsync(s =>
				s.CampId == s.CampId
				&& s.Id != id
				&& s.Date.Day == newSession.Date.Day && s.Date.Month == newSession.Date.Month);
				response.Comment += response.Success == false ? "Conflict in data\n" : string.Empty;
			}

			return response;
		}
	}
}
