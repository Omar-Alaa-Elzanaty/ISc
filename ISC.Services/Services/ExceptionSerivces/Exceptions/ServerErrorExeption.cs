using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Services.ExceptionSerivces.Exceptions
{
	public class ServerErrorExeption : Exception
	{
		public ServerErrorExeption(string? message) : base(message)
		{
		}
	}
}
