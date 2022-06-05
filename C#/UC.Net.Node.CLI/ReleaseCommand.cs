using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;

namespace UC.Net.Node.CLI
{
	/// <summary>
	/// Usage: 
	///		   release publish 
	///							by = ACCOUNT 
	///							[password = PASSWORD]
	/// </summary>
	public class ReleaseCommand : Command
	{
		public const string Keyword = "release";

		public ReleaseCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{

				case "declare" : 
					return Send(() => Client.Enqueue(new ReleaseManifest (	GetPrivate("by", "password"), 
																			ReleaseAddress.Parse(GetString("address")),
																			GetString("channel"), 
																			GetVersion("previous"),
																			
																			GetLong("csize"),
																			GetHexBytes("chash"),
																			GetStringOrEmpty("cdependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i =>  ReleaseAddress.Parse(i)),

																			GetVersion("iminimal"),
																			GetLong("isize"),
																			GetHexBytes("ihash"),
																			GetStringOrEmpty("idependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i =>  ReleaseAddress.Parse(i))
																			)));

				case "publish" :
				{
					var sources = GetString("sources").Split(',');
					var r = ReleaseAddress.Parse(GetString("address"));
					
 					var files = new Dictionary<string, string>();

					foreach(var i in sources)
					{
						var sd = i.Split('=');
						var s = sd[0];
						var d = sd.Length == 2 ? sd[1] : null;

						if(d == null)
						{
							if(Directory.Exists(s))
							{
			 					foreach(var e in Directory.EnumerateFileSystemEntries(s, "*", new EnumerationOptions {RecurseSubdirectories = true}))
			 						files[e] = e.Substring(s.Length + 1).Replace(Path.DirectorySeparatorChar, '/');
							} 
							else
								files[s] = Path.GetFileName(s);
						} 
						else
						{
							if(Directory.Exists(s))
							{
			 					foreach(var e in Directory.EnumerateFileSystemEntries(s, "*", new EnumerationOptions {RecurseSubdirectories = true}))
			 						files[e] = Path.Join(d, e.Substring(s.Length + 1).Replace(Path.DirectorySeparatorChar, '/'));
							} 
							else
								files[s] = d;
						}
					}

					var cpkg = Client.Filebase.Add(r, Distribution.Complete, files);
					var ipkg = Client.Filebase.AddIncremental(r, files, out Version previous, out Version minimal);

					return Send(() => Client.Enqueue(new ReleaseManifest (	GetPrivate("by", "password"), 
																			r,
																			GetString("channel"), 
																			previous,
																			
																			new FileInfo(cpkg).Length,
																			Cryptography.Current.Hash(File.ReadAllBytes(cpkg)),
																			GetStringOrEmpty("cdependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => ReleaseAddress.Parse(i)),

																			minimal,
																			ipkg != null ? new FileInfo(ipkg).Length : 0,
																			ipkg != null ? Cryptography.Current.Hash(File.ReadAllBytes(ipkg)) : null,
																			GetStringOrEmpty("idependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => ReleaseAddress.Parse(i))
																			)));
				}

		   		case "status" :
				{
					var r = Client.ConnectToNode().QueryRelease(ReleaseQuery.Parse(GetString("query")), Args.Has("confirmed"));

					foreach(var item in r.Xons)
					{
						Dump(item);
					}

					return r;
				}
				
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
