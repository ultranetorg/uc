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
	public class LaunchArguments
	{
		public static void Parse(Xon args, Action<string, string> f)
		{
			foreach(var i in args.Nodes)
			{
				f(i.Name, i.String);
			}
		}
	}

	public class MainArguments : LaunchArguments
	{
		public string			Profile;
		public string			Secrets;
		public string			Zone;

		public MainArguments(Xon boot, Xon cmd)
		{
			Zone = boot.GetString("Zone");
			Profile = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UC.Dun", Zone);

			Parse(cmd, (n,v) => { 
									switch(n)
									{
										case "profile":	Profile	= v; break;
										case "secrets":	Secrets	= v; break;
									}
								});
		}
	}

	public class CoreArguments : LaunchArguments
	{
		public int				Port = -1;
		public IPAddress		IP;
		public CoreArguments(Xon cmd)
		{
			if(cmd.Has("core"))
				Parse(cmd.One("core"), (n,v) => { 
														switch(n)
														{
															case "port":	Port	= int.Parse(v); break;
															case "ip":		IP		= IPAddress.Parse(v); break;
														}
													});
			}
	}

	public class VaultArguments : LaunchArguments
	{
		public List<string>		Accounts = new();

		public VaultArguments(Xon cmd)
		{
			if(cmd.Has("vault"))
				Parse(cmd.One("vault"), (n,v) => {	switch(n)
													{
														case "account":
															Accounts.Add(v); 
															break;
													}
												 });
		}
	}

	public class RpcArguments : LaunchArguments
	{
		public string AccessKey;

		public RpcArguments(Xon args)
		{
			if(args.Has("rpc"))
				Parse(args.One("rpc"), (n, v) => {	switch(n)
													{
														case "accesskey":
															AccessKey = v; 
															break;
													}
												 });
		}
	}

	public class BootArguments
	{
		public MainArguments			Main;
		public VaultArguments			Vault;
		public CoreArguments		Core;
		public RpcArguments				Rpc;

		public BootArguments(Xon boot, Xon cmd)
		{
			Main		= new(boot, cmd);
			Vault		= new(cmd);
			Core	= new(cmd);
			Rpc			= new(cmd);
		}
	}
}
