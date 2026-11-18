using System;
using System.Text.RegularExpressions;
using System.IO;

using O;

namespace TestAutomation
{
	class ShouldFailException : Exception
	{
		public ShouldFailException()
		{ }
		public ShouldFailException(string message) : base(message)
		{ }
		public ShouldFailException(string message, Exception inner) : base(message, inner)
		{ }
	}
}

namespace CommonUtils
{
	public enum StandardOutputStreamType { Out, Err }

	class Common
	{
	}
}

namespace CommonUtils.Ext
{
	public static class Extensions
	{
		/// <summary> Replace all the symbols xml does not like with emptyness. </summary>
		public static string PrepXml(this string s)
		{
			return Regex.Replace(s, @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]", string.Empty);
		}

		/// <summary>
		/// Extension for DirectoryInfo to recursively delete all the contents of the directory.
		/// </summary>
		/// <param name="directory"></param>
		public static void Empty(this DirectoryInfo directory)
		{
			foreach (FileInfo file in directory.GetFiles()) file.Delete();
			foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
		}

		/// <summary>
		/// Get full stack trace(recursively for each innerException) as a one print-ready string.
		/// </summary>
		/// <returns>Print-ready string containing full stack-trace</returns>
		public static string GetFullStack(this Exception? e)
		{
			if (e?.StackTrace == null) return null;
			string stack = $"\nMessage: {e.Message}\n" + e.StackTrace;
			Exception? inner = e?.InnerException;
			while (true) {
				if (inner == null) break;
				stack += $"\nMessage: {inner.Message}\n";
				stack += inner.StackTrace;
				inner = inner.InnerException;
			}
			return stack;
		}
	}
}
