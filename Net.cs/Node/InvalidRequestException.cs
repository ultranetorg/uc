using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	class InvalidRequestException : Exception
	{
		public InvalidRequestException(string message) : base(message)
		{
		}
	}
}
