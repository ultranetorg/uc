namespace Uccs.Net;

public class NetBoot : Boot
{
	public Zone				Zone;
	public const string		DefaultDirectoryName = "Uos";

	public NetBoot()
	{
	}

	public NetBoot(string exedir) : base(exedir)
	{
		if(Commnand.Has("zone"))
			Zone = Enum.Parse<Zone>(Commnand.Get<string>("zone"));
		else if(Default?.Has("Zone") == true)
			Zone = Enum.Parse<Zone>(Default.Get<string>("Zone"));
		else
			Zone = Zone.Test;

		if(Commnand.Has("profile"))
			Profile = Commnand.Get<string>("profile");
		else
			Profile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultDirectoryName, Zone.ToString());

	}
}
