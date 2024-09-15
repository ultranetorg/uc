namespace Uccs.Net
{
	public class Boot
	{
		public const string	FileName = "Uos.boot";

		public Xon			Commnand;
	 	public string		Profile;
		public string		Zone;

		public Boot()
		{
		}

		public Boot(string exedir)
		{
			var b = new Xon(File.ReadAllText(Path.Combine(exedir, FileName)));
			Commnand = new Xon(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));

			if(Commnand.Has("interzone"))
				Zone = Commnand.Get<string>("interzone");
			else
				Zone = b.Get<string>("Interzone");

			if(Commnand.Has("profile"))
				Profile = Commnand.Get<string>("profile");
			else
				Profile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UO.Uos", Zone);
		}
	}
}
 