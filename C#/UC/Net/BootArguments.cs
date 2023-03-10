using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Nethereum.Util;
using System.Reflection;

namespace UC.Net
{
	public class BootArguments
	{
	 	public string	Profile;
	 	public string	Secrets;
	 	public string	Zone;

		public BootArguments()
		{
		}

		public BootArguments(Xon boot, Xon cmd)
		{
 			Zone = boot.GetString("Zone");
 			Profile = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UO.Sun", Zone);
		
 			Parse(cmd, (n,v) => { 
 									switch(n)
 									{
										case "zone":	Zone	= v; break;
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
