namespace Uccs;

public abstract class Boot
{
	public const string	FileName = "Boot.settings";

	public Xon			Default;
	public Xon			Commnand;
 	public string		Profile;

	public Boot()
	{
	}

	public Boot(string exedir)
	{
		var f = Path.Combine(exedir, FileName);
		
		if(File.Exists(f))
			Default = new Xon(File.ReadAllText(f));

		Commnand = new Xon(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));
	}
}
 