namespace Uccs.Net;

public class Boot
{
	public const string	FileName = "Uos.boot";

	public Xon			Commnand;
 	public string		Profile;
	public Zone			Zone;

	public Boot()
	{
	}

	public Boot(string exedir)
	{
		var b = new Xon(File.ReadAllText(Path.Combine(exedir, FileName)));
		Commnand = new Xon(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));

		if(Commnand.Has("zone"))
			Zone = Enum.Parse<Zone>(Commnand.Get<string>("zone"));
		else
			Zone = Enum.Parse<Zone>(b.Get<string>("Zone"));

		if(Commnand.Has("profile"))
			Profile = Commnand.Get<string>("profile");
		else
			Profile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UO.Uos", Zone.ToString());
	}
}
 