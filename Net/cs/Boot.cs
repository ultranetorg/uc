namespace Uccs.Net
{
	public class Boot
	{
		public const string	FileName = "Uos.boot";

		public Xon			Commnand;
	 	public string		Profile;
		public Land			Land;

		public Boot()
		{
		}

		public Boot(string exedir)
		{
			var b = new Xon(File.ReadAllText(Path.Combine(exedir, FileName)));
			Commnand = new Xon(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));

			if(Commnand.Has("nexus"))
				Land = Enum.Parse<Land>(Commnand.Get<string>("nexus"));
			else
				Land = Enum.Parse<Land>(b.Get<string>("Land"));

			if(Commnand.Has("profile"))
				Profile = Commnand.Get<string>("profile");
			else
				Profile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UO.Uos", Land.ToString());
		}
	}
}
 