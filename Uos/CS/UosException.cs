using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{

	public abstract class UosException : Exception
	{
		public UosException()
		{
		}

		public UosException(string message) : base(message)
		{
		}
	}	
}
