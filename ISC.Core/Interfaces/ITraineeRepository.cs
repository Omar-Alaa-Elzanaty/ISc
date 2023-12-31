﻿using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Interfaces
{
	public interface ITraineeRepository:IBaseRepository<Trainee>
	{
		 Task<Trainee> getByUserIdAsync(string userId);
		Task<Camp> getCampofTrainee(int id);
		Task<Camp?> getCampofTrainee(string userId);
	}
}
