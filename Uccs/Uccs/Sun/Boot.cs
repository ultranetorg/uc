using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Nethereum.Util;
using System.Reflection;

namespace Uccs.Net
{
	public class Boot
	{
	 	public string	Profile;
	 	public string	Secrets;
		public Xon		Commnand;
		public Zone		Zone;

		string			ZonePath;

		public Boot()
		{
		}

		public Boot(string exedir)
		{
			var b = new XonDocument(File.ReadAllText(Path.Combine(exedir, "Boot.xon")));
			Commnand = new XonDocument(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));

			Load(b, Commnand);

			var z = Zone.OfficialByName(ZonePath);

			if(z != null)
			{
				Zone = z;
			}
			else if(Path.IsPathRooted(ZonePath))
			{
				Zone = new Zone();
				Zone.Load(ZonePath);
			}
			else
			{
				var std = Path.Join(exedir, ZonePath + ".zone");
	
				if(File.Exists(std))
				{
					Zone = new Zone();
					Zone.Load(std);
				}
			}

			if(Profile == null)
			{
				Profile = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UO.Sun", Zone.Name);
			}
		}

		public void Load(Xon boot, Xon cmd)
		{
 			ZonePath = boot.Get<string>("Zone");
		
 			Parse(cmd, (n,v) => { 
 									switch(n)
 									{
										case "zone":	ZonePath= v; break;
 										case "profile":	Profile	= v; break;
 										case "secrets":	Secrets	= v; break;
 									}
 								});			
		}

		public static void Parse(Xon args, Action<string, string> f)
		{
			foreach(var i in args.Nodes)
			{
				f(i.Name, i.String);
			}
		}
	}
}
