using ISC.Core.Interfaces;
using ISC.Core.ModelsDtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	internal class SessionFeedbackRepository : BaseRepository<SessionFeedback>, ISessionFeedbackRepository
	{
		private readonly DataBase _Context;
        public SessionFeedbackRepository(DataBase context):base(context)
        {
            _Context = context;
        }
		public async Task<List<SessionFeedback>> getTopAsync(int count)
		{
			var Feedbacks=await _Context.SessionsFeedbacks.OrderByDescending(feed=>feed.Rate).Take(count).ToListAsync();
			return Feedbacks;
		}
	}
}
