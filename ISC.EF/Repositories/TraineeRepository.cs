using ISC.Core.Interfaces;
using ISC.Core.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.Repositories
{
	internal class TraineeRepository:BaseRepository<Trainee>,ITraineeRepository
	{
        private readonly DataBase _Context;
        public TraineeRepository(DataBase context):base(context) 
        {
            _Context = context;
        }
	}
}
