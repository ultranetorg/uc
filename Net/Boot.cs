using System;
using System.IO;
using System.Linq;
using Uccs.Net;

namespace Uccs.Net
{
	public class Boot
	{
		public Xon		Commnand;
	 	public string	Profile;
		public Zone		Zone;

		public Boot()
		{
		}

		public Boot(string exedir)
		{
			var b = new XonDocument(File.ReadAllText(Path.Combine(exedir, "Uos.boot")));
			Commnand = new XonDocument(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));

			if(Commnand.Has("zone"))
				Zone = Zone.OfficialByName(Commnand.Get<string>("zone"));
			else
				Zone = Zone.OfficialByName(b.Get<string>("Zone"));

			if(Commnand.Has("profile"))
				Profile = Commnand.Get<string>("profile");
			else
				Profile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UO.Sun", Zone.Name);
		}
	}
}
