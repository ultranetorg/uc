using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Threading;
using Uccs.Net;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class ResourceCommand : Command
	{
		public const string Keyword = "package";

		public ResourceCommand(Zone zone, Settings settings, Workflow workflow, Func<Net.Sun> sun, Xon args) : base(zone, settings, workflow, sun, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "c" : 
				case "create" : 
				{	
					var r =	new ResourceCreation(	Sun.Vault.GetKey(AccountAddress.Parse(GetString("by"))), 
													ResourceAddress.Parse(GetString("address")),
													byte.Parse(GetString("years")),
													Args.Has("flags")		? Enum.Parse<ResourceFlags>(GetString("flags")) : ResourceFlags.Null,
													Args.Has("type")		? Enum.Parse<ResourceType>(GetString("type")) : ResourceType.Null,
													Args.Has("data")		? GetHexBytes("data") : null,
													Args.Has("parent")		? GetString("parent") : null, 
													Args.Has("analysisfee") ? Coin.ParseDecimal(Args.GetString("analysisfee")) : Coin.Zero);

					return Sun.Enqueue(r, GetAwaitStage(), Workflow);
				}

				case "u" : 
				case "update" : 
				{	
					var r =	new ResourceUpdation(Sun.Vault.GetKey(AccountAddress.Parse(GetString("by"))), ResourceAddress.Parse(GetString("address")));

					if(Args.Has("years"))		r.Change(byte.Parse(GetString("years")));
					if(Args.Has("flags"))		r.Change(Enum.Parse<ResourceFlags>(GetString("flags")));
					if(Args.Has("type"))		r.Change(Enum.Parse<ResourceType>(GetString("type")));
					if(Args.Has("data"))		r.Change(GetHexBytes("data"));
					if(Args.Has("parent"))		r.Change(GetString("parent"));
					if(Args.Has("analysisfee")) r.Change(Coin.ParseDecimal(Args.GetString("analysisfee")));
					if(Args.Has("recursive"))	r.ChangeRecursive();
					
					return Sun.Enqueue(r, GetAwaitStage(), Workflow);
				}

				case "b" :
				case "build" :
				{	
					ReleaseBaseItem rbi = null;

					if(Args.Has("source"))
						rbi = Sun.Resources.Add(GetResourceAddress("address"), GetString("source"), Workflow);
					else if(Args.Has("sources"))
						rbi = Sun.Resources.Add(GetResourceAddress("address"), GetString("sources").Split(','), Workflow);
					else
						throw new SyntaxException("Unknown arguments");

					Workflow.Log?.Report(this, $"Hash={rbi.Hash.ToHex()}");

					return rbi;
				}

				case "d" :
				case "download" :
				{
					var a = GetResourceAddress("address");

					var r = Sun.Call(c => c.FindResource(a), Workflow).Resource;

					Sun.Resources.Add(a, r.Data);

					if(r.Type == ResourceType.File)
					{
						FileDownload d;
						
						lock(Sun.Resources.Lock)
							d = Sun.Resources.DownloadFile(a, r.Data, "f", r.Data, null, Workflow);
	
						if(d != null)
						{
							while(!d.Succeeded)
							{
								Thread.Sleep(500);

								lock(Sun.Resources.Lock)
								{ 
									Workflow.Log?.Report(this, $"{d.CompletedLength}/{d.Length} bytes, {d.SeedCollector.Hubs.Count} hubs, {d.SeedCollector.Seeds.Count} seeds");
								}
							}
						} 
						else
							Workflow.Log?.Report(this, $"Already downloaded");
				
						return d;
					}
					else if(r.Type == ResourceType.Directory)
					{
						DirectoryDownload d;
						
						lock(Sun.Resources.Lock)
							d = Sun.Resources.DownloadDirectory(a, Workflow);
	
						if(d != null)
						{
							while(!d.Succeeded)
							{
								lock(Sun.Resources.Lock)
								{ 
									Workflow.Log?.Report(this, $"{d.CompletedCount} downloaded, {d.Files.Count} left, {d.SeedCollector?.Hubs.Count} hubs, {d.SeedCollector?.Seeds.Count} seeds");
								}
								
								Thread.Sleep(500);
							}
						} 
						else
							Workflow.Log?.Report(this, $"Already downloaded");
				
						return d;
					}

					throw new NotImplementedException();
				}

				case "i" :
		   		case "info" :
				{
					try
					{
						var r = Sun.Call(i => i.FindResource(GetResourceAddress("address")), Workflow);
	
						Workflow.Log?.Report(this, GetString("address"));

						Dump(r.Resource);

						var e = Sun.Call(i => i.EnumerateSubresources(GetResourceAddress("address")), Workflow);

						if(e.Resources.Any())
						{
							foreach(var i in e.Resources)
							{
								Workflow.Log?.Report(this, "      " + i);
							}
						}

						return r;
					}
					catch(RdcEntityException ex)
					{
						Workflow.Log?.Report(this, ex.Message);
						return null;
					}
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
