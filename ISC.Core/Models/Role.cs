﻿
namespace ISC.Core.Models
{
	public class Role
	{
        public const string LEADER = "Leader";
		public const string TRAINEE = "Trainee";
		public const string MENTOR = "Mentor";
		public const string HOC = "Head of training";
		public const string INSTRUCTOR = "Instructor";
	}
	public enum SheetType: byte
	{
		Practice=0,
		Contest=1
	}
}
