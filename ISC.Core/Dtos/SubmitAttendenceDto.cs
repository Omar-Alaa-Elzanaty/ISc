﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
	public class SubmitAttendenceDto
	{
		public int TraineeId { get; set; }
		public bool IsAttend { get; set; }
	}
}
