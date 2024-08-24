using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Rdn
{
	public class PackageException : Exception
	{
		public PackageException()
		{
		}

		public PackageException(string message) : base(message)
		{
		}
	}
}
