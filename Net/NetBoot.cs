namespace Uccs.Net;

public class NetBoot : Boot
{
	public Zone						Zone;
	public const string				DefaultDirectoryName = "Uos";
	public static string			DefaultPath(Zone zone) => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultDirectoryName, zone.ToString());

	public const string				ZoneEnvironmentKey = "UC_Zone";
	public const string				ProfileEnvironmentKey = "UC_Profile";

	public NetBoot()
	{
	}

	public NetBoot(string exedir) : base(exedir)
	{
		if(Commnand.Has("zone"))
			Zone = Enum.Parse<Zone>(Commnand.Get<string>("zone"));
		else if(Environment.GetEnvironmentVariable(ZoneEnvironmentKey) != null)
			Zone = Enum.Parse<Zone>(Environment.GetEnvironmentVariable(ZoneEnvironmentKey));
		else if(Default?.Has("Zone") == true)
			Zone = Enum.Parse<Zone>(Default.Get<string>("Zone"));
		else
			Zone = Zone.Test;

		if(Commnand.Has("profile"))
			Profile = Commnand.Get<string>("profile");
		else if(Environment.GetEnvironmentVariable(ProfileEnvironmentKey) != null)
			Profile = Environment.GetEnvironmentVariable(ProfileEnvironmentKey);
		else
			Profile = DefaultPath(Zone);

	}
}
