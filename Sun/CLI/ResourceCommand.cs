using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Threading;
using Uccs.Net;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Threading.Tasks;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class ResourceCommand : Command
	{
		public const string Keyword = "resource";

		public ResourceCommand(Zone zone, Settings settings, Workflow workflow, Net.Sun sun, Xon args) : base(zone, settings, workflow, sun, args)
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
					return new ResourceCreation(ResourceAddress.Parse(GetString("address")),
												byte.Parse(GetString("years")),
												Args.Has("flags")		? Enum.Parse<ResourceFlags>(GetString("flags")) : ResourceFlags.Null,
												Args.Has("type")		? Enum.Parse<ResourceType>(GetString("type")) : ResourceType.Null,
												Args.Has("data")		? GetHexBytes("data") : null,
												Args.Has("parent")		? GetString("parent") : null, 
												Args.Has("analysisfee") ? Coin.ParseDecimal(GetString("analysisfee")) : Coin.Zero);
				}

				case "u" : 
				case "update" : 
				{	
					var r =	new ResourceUpdation(ResourceAddress.Parse(GetString("address")));

					if(Args.Has("years"))		r.Change(byte.Parse(GetString("years")));
					if(Args.Has("flags"))		r.Change(Enum.Parse<ResourceFlags>(GetString("flags")));
					if(Args.Has("type"))		r.Change(Enum.Parse<ResourceType>(GetString("type")));
					if(Args.Has("data"))		r.Change(GetHexBytes("data"));
					if(Args.Has("parent"))		r.Change(GetString("parent"));
					if(Args.Has("analysisfee")) r.Change(Coin.ParseDecimal(GetString("analysisfee")));
					if(Args.Has("recursive"))	r.ChangeRecursive();
					
					return r;
				}

				case "b" :
				case "build" :
				{	
					Release rbi = null;

					if(Args.Has("source"))
						rbi = Sun.ResourceHub.Add(GetResourceAddress("address"), GetString("source"), Workflow);
					else if(Args.Has("sources"))
						rbi = Sun.ResourceHub.Add(GetResourceAddress("address"), GetString("sources").Split(','), Workflow);
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
					
					Release rz;

					lock(Sun.ResourceHub.Lock)
						rz = Sun.ResourceHub.Find(a, r.Data) ?? Sun.ResourceHub.Add(a, r.Data);
	
					if(r.Type == ResourceType.File)
					{
						FileDownload d;
							
						lock(Sun.ResourceHub.Lock)
							d = Sun.ResourceHub.DownloadFile(rz, "f", r.Data, null, Workflow);
		
						if(d != null)
						{
							do
							{
								Task.WaitAny(new Task[] {d.Task}, 500, Workflow.Cancellation);
	
								lock(d.Lock)
								{ 
									if(d.File != null)
										Workflow.Log?.Report(this, $"{d.DownloadedLength}/{d.Length} bytes, {d.CurrentPieces.Count} threads, {d.SeedCollector.Hubs.Count} hubs, {d.SeedCollector.Seeds.Count} seeds");
									else
										Workflow.Log?.Report(this, $"?/? bytes, {d.SeedCollector.Hubs.Count} hubs, {d.SeedCollector.Seeds.Count} seeds");
								}
							}
							while(!d.Task.IsCompleted && Workflow.Active);
						} 
						else
							Workflow.Log?.Report(this, $"Already downloaded");
					
						return d;
					}
					else if(r.Type == ResourceType.Directory)
					{
						DirectoryDownload d;
							
						lock(Sun.ResourceHub.Lock)
							d = Sun.ResourceHub.DownloadDirectory(rz, Workflow);
		
						if(d != null)
						{
							void report() => Workflow.Log?.Report(this, $"{d.CompletedCount}/{d.TotalCount} files, {d.SeedCollector?.Hubs.Count} hubs, {d.SeedCollector?.Seeds.Count} seeds");
	
							do
							{
								Task.WaitAny(new Task[] {d.Task}, 500, Workflow.Cancellation);
	
								report();
							}
							while(!d.Task.IsCompleted && Workflow.Active);
						} 
						else
							Workflow.Log?.Report(this, $"Already downloaded");
					
						return d;
					}
	
					throw new NotSupportedException();
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

						return r.Resource;
					}
					catch(RdcEntityException ex)
					{
						Workflow.Log?.Report(this, ex.Message);
					}
					
					return null;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
