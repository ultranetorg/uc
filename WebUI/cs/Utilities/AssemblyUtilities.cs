using System.Reflection;

namespace Uccs.WebUI.Utilities;

public static class AssemblyUtilities
{
	public static string? GetDirectoryName()
	{
		string? location = Assembly.GetEntryAssembly()?.Location;
		return Path.GetDirectoryName(location);
	}
}
