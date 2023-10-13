using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class ResourceCommand : Command
	{
		public const string Keyword = "resource";

		public ResourceCommand(Program program, Xon args) : base(program, args)
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
					return new ResourceCreation(ResourceAddress.Parse(Args.Nodes[1].Name),
												byte.Parse(GetString("years")),
												Args.Has("flags")		? Enum.Parse<ResourceFlags>(GetString("flags")) : ResourceFlags.None,
												Args.Has("type")		? Enum.Parse<ResourceType>(GetString("type")) : ResourceType.None,
												Args.Has("data")		? GetHexBytes("data") : null,
												Args.Has("parent")		? GetString("parent") : null, 
												Args.Has("analysisfee") ? Money.ParseDecimal(GetString("analysisfee")) : Money.Zero);
				}

				case "u" : 
				case "update" : 
				{	
					var r =	new ResourceUpdation(ResourceAddress.Parse(Args.Nodes[1].Name));

					if(Args.Has("years"))		r.Change(byte.Parse(GetString("years")));
					if(Args.Has("flags"))		r.Change(Enum.Parse<ResourceFlags>(GetString("flags")));
					if(Args.Has("type"))		r.Change(Enum.Parse<ResourceType>(GetString("type")));
					if(Args.Has("data"))		r.Change(GetHexBytes("data"));
					if(Args.Has("parent"))		r.Change(GetString("parent"));
					if(Args.Has("analysisfee")) r.Change(Money.ParseDecimal(GetString("analysisfee")));
					if(Args.Has("recursive"))	r.ChangeRecursive();
					
					return r;
				}

				case "b" :
				case "build" :
				{
					if(!Args.Has("source") && !Args.Has("sources"))
						throw new SyntaxException("Unknown arguments");

					var h = Program.Call<byte[]>(new ResourceBuildCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name),
																		FilePath = GetString("source", null),
																		Sources = GetString("sources", null)?.Split(',')});

					Workflow.Log?.Report(this, $"Hash={h.ToHex()}");

					return null;
				}

				case "d" :
				case "download" :
				{
					var a = ResourceAddress.Parse(Args.Nodes[1].Name);
		
					var h = Program.Call<byte[]>(new ResourceDownloadCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});

					try
					{
						ResourceDownloadProgress d = null;
						
						while(Workflow.Active)
						{

							d = Program.Call<ResourceDownloadProgress>(new ResourceDownloadProgressCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name), Hash = h});

							if(d == null)
								break;

							Workflow.Log?.Report(this, d.ToString());

							Thread.Sleep(500);
						}
					}
					catch(OperationCanceledException)
					{
					}

					return null;



// 					var r = Sun.Call(c => c.FindResource(a), Workflow).Resource;
// 					
// 					Release rz;
// 
// 					lock(Sun.ResourceHub.Lock)
// 						rz = Sun.ResourceHub.Find(a, r.Data) ?? Sun.ResourceHub.Add(a, r.Data);
// 	
// 					if(r.Type == ResourceType.File)
// 					{
// 						FileDownload d;
// 							
// 						lock(Sun.ResourceHub.Lock)
// 							d = Sun.ResourceHub.DownloadFile(rz, "f", r.Data, null, Workflow);
// 		
// 						if(d != null)
// 						{
// 							do
// 							{
// 								Task.WaitAny(new Task[] {d.Task}, 500, Workflow.Cancellation);
// 	
// 								lock(d.Lock)
// 								{ 
// 									if(d.File != null)
// 										Workflow.Log?.Report(this, $"{d.DownloadedLength}/{d.Length} bytes, {d.CurrentPieces.Count} threads, {d.SeedCollector.Hubs.Count} hubs, {d.SeedCollector.Seeds.Count} seeds");
// 									else
// 										Workflow.Log?.Report(this, $"?/? bytes, {d.SeedCollector.Hubs.Count} hubs, {d.SeedCollector.Seeds.Count} seeds");
// 								}
// 							}
// 							while(!d.Task.IsCompleted && Workflow.Active);
// 						} 
// 						else
// 							Workflow.Log?.Report(this, $"Already downloaded");
// 					
// 						return d;
// 					}
// 					else if(r.Type == ResourceType.Directory)
// 					{
// 						DirectoryDownload d;
// 							
// 						lock(Sun.ResourceHub.Lock)
// 							d = Sun.ResourceHub.DownloadDirectory(rz, Workflow);
// 		
// 						if(d != null)
// 						{
// 							void report() => Workflow.Log?.Report(this, $"{d.CompletedCount}/{d.TotalCount} files, {d.SeedCollector?.Hubs.Count} hubs, {d.SeedCollector?.Seeds.Count} seeds");
// 	
// 							do
// 							{
// 								Task.WaitAny(new Task[] {d.Task}, 500, Workflow.Cancellation);
// 	
// 								report();
// 							}
// 							while(!d.Task.IsCompleted && Workflow.Active);
// 						} 
// 						else
// 							Workflow.Log?.Report(this, $"Already downloaded");
// 					
// 						return d;
// 					}
// 	
// 					throw new NotSupportedException();
				}

				case "i" :
		   		case "info" :
				{
					var r = Program.Call<ResourceInfo>(new ResourceInfoCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name), 
																			 Hash = Args.Get<byte[]>("hash", null)});

					Dump(r);

					//var e = Program.Rdc<SubresourcesResponse>(new SubresourcesRequest {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});
					//
					//if(e.Resources.Any())
					//{
					//	foreach(var i in e.Resources)
					//	{
					//		Workflow.Log?.Report(this, "    " + i);
					//	}
					//}

					return r;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
