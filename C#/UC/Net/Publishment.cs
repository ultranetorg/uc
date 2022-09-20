using System;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Management;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UC;

namespace UC.Net
{
/*
	public class Publishment
	{
		Core		Core;
		Settings		Settings;
		Log				Log;
		IpfsClient		Ipfs;
		string			IpfsStatus;
		string			ManifestPath;
		PrivateAccount	Publisher;
		Coin			Fee;

		public Publishment(Core d, Log log, PrivateAccount publisher, Coin fee, string manifestpath)
		{
			Core		= d;
			Settings		= d.Settings;
			Log				= log;
			ManifestPath	= manifestpath;
			Publisher		= publisher;
			Fee				= fee;
				
			RequestIpfs().Wait();
		}

		async public Task RequestIpfs()
		{
			IpfsStatus = "Connecting...";

			Ipfs = new IpfsClient(Settings.Ipfs.Host);

			if(Settings.Ipfs != null)
			{
				bool run = false;

				foreach(var i in Process.GetProcessesByName("ipfs"))
				{
					foreach(ProcessModule m in i.Modules)
					{
						if(m.FileName.ToLower() == Settings.Ipfs.Path.ToLower()/ * && GetCommandLine(i).EndsWith("daemon")* /)
						{
							run = true;
							break;
						}
					}
				}
		
				if(!run)
				{
					var p = new Process();
					p.StartInfo.FileName = Settings.Ipfs.Path;
					p.StartInfo.Arguments = "daemon";
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.Start();
				}
			}

			while(true)
			{
				try
				{
					var r = await Ipfs.VersionAsync();
					IpfsStatus = "Online";
					Log.Report(this, "Status", IpfsStatus);
					break;
				}
				catch(HttpRequestException e)
				{
					IpfsStatus = "Connecting failed - " + e.Message + "... Trying again ...";
					Log.Report(this, "Status", IpfsStatus);
				}
			}
		}

		private string GetCommandLine(Process process)
		{
			throw new NotImplementedException();
			///using(var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
			///using(var objects = searcher.Get())
			///{
			///	return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
			///}
		}

		public string ResolvePath(string p)
		{
			if(!Path.IsPathRooted(p))
				return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ManifestPath), p));
			else
				return p;
		}

		async public Task<XonDocument> Preprocess()
		{
			var d = new XonDocument(new XonTextReader(File.ReadAllText(ManifestPath)));
			var r = new XonDocument();
	
			Action<Xon, Xon> f = null;

			f = async (a, b) =>
				{
					foreach(var i in a.Nodes)
					{
						Xon c = null;
	
						if(i.Name.StartsWith("#"))
						{
							var name = i.Name.Substring(1);
							if(name == "Version")
							{
								var v = FileVersionInfo.GetVersionInfo(ResolvePath(i.GetString("From")));
								b.Nodes.Add(new Xon{ Name = name, Value = v.ProductVersion.Replace(',', '.')});
								continue;
							}
							if(name == "Vendor")
							{
								var v = FileVersionInfo.GetVersionInfo(ResolvePath(i.GetString("From")));
								b.Nodes.Add(new Xon{ Name = name, Value = v.CompanyName });
								continue;
							}
							if(name == "Package")
							{
								b.Nodes.AddRange(await ProcessComponents(i));
								continue;
							}
						}
						
						if(c == null)
						{
							c = new Xon{ Name = i.Name, Value = i.Value };
						}
	
						b.Nodes.Add(c);
	
						f(i, c);
					}
				};
				
			return await Task.Run(() => {
											f(d, r); 
											return d; 
										});
		}

		async Task<List<Xon>> ProcessComponents(Xon on) 
		{
			var r = new List<Xon>();

			var source = ResolvePath(on.GetString("From"));

			Func<string, Task> scan = null;
			
			scan =	async (p) =>
					{
						foreach(var i in Directory.GetFileSystemEntries(p))
						{
							if(File.Exists(i))
							{
								var cid = await Ipfs.FileSystem.AddFileAsync(i);

								Log.Report(this, "File added", i);
	
								var f = new Xon(cid.Id.ToString()) ;
								r.Add(f);
							}

							if(Directory.Exists(i))
							{ 
								await scan(i);
							}
						}
					};

			await scan(source);

			return r;
		}

		async public Task Publish()
		{
			var manifest = await Preprocess();
			
			var w = new StringWriter();
			manifest.Save(new XonTextWriter(w));
			//Core.PublishRelease(Publisher, w.ToString());
		}
	}*/
}
