namespace Uccs;

public abstract class Boot
{
	public const string	FileName = "Uos.boot";

	public Xon			Default;
	public Xon			Commnand;
 	public string		Profile;

	public Boot()
	{
	}

	public Boot(string exedir)
	{
		Default = new Xon(File.ReadAllText(Path.Combine(exedir, FileName)));
		Commnand = new Xon(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));
	}
}
 