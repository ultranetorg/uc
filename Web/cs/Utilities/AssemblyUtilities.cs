using System.Reflection;

namespace Uccs.Web.Utilities;

public static class AssemblyUtilities
{
	public static string? GetDirectoryName()
	{
		string? location = Assembly.GetEntryAssembly()?.Location;
		return Path.GetDirectoryName(location);
	}
}
