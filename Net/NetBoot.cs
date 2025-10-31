namespace Uccs.Net;

public class NetBoot : Boot
{
	public Zone			Zone;

	public NetBoot()
	{
	}

	public NetBoot(string exedir)
	{
		if(Commnand.Has("zone"))
			Zone = Enum.Parse<Zone>(Commnand.Get<string>("zone"));
		else
			Zone = Enum.Parse<Zone>(Default.Get<string>("Zone"));

		if(Commnand.Has("profile"))
			Profile = Commnand.Get<string>("profile");
		else
			Profile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UO.Uos", Zone.ToString());

	}
}
