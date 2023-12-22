using System;
using System.Collections.Generic;
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
												Args.Has("flags")	? Enum.Parse<ResourceFlags>(GetString("flags")) : ResourceFlags.None,
												GetHexBytes("data", false),
												GetString("parent", false));
				}

				case "u" : 
				case "update" : 
				{	
					var r =	new ResourceUpdation(ResourceAddress.Parse(Args.Nodes[1].Name));

					if(Args.Has("flags"))		r.Change(Enum.Parse<ResourceFlags>(GetString("flags")));
					if(Args.Has("data"))		r.Change(GetHexBytes("data"));
					if(Args.Has("parent"))		r.Change(GetString("parent"));
					if(Args.Has("recursive"))	r.ChangeRecursive();
					
					return r;
				}

				case "e" :
		   		case "entity" :
				{
					var r = Program.Rdc<ResourceResponse>(new ResourceRequest {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});
					
					Dump(r.Resource);

					return r;
				}

				case "ls" : 
				case "list" : 
				{	
					var r = Program.Api<IEnumerable<LocalResource>>(new QueryLocalResourcesCall {Query = Args.Nodes[1].Name});
					
					Dump(	r, 
							new string[] {"Address", "Releases", "Latest Type", "Latest Data", "Latest Length"}, 
							new Func<LocalResource, string>[]{	i => i.Address.ToString(),
																i => i.Datas.Count.ToString(),
																i => i.Last.Type.ToString(),
																i => i.Last.Data.ToHex(32),
																i => i.Last.Data.Length.ToString() });
					return r;
				}

				case "l" : 
				case "local" : 
				{	
					var r = Program.Api<LocalResource>(new LocalResourceCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});
					
					if(r != null)
					{
						Dump(	r.Datas, 
								new string[] {"Type", "Data", "Length"}, 
								new Func<ResourceData, string>[] {	i => i.Type.ToString(), 
																	i => i.Data.ToHex(32), 
																	i => i.Data.Length.ToString() });
						return r;
					}
					else
						throw new Exception("Resource not found");
				}


				case "d" :
				case "download" :
				{
					var a = ResourceAddress.Parse(Args.Nodes[1].Name);
					var h = Program.Rdc<ResourceResponse>(new ResourceRequest {Resource = a}).Resource.Data;

					lock(Program.Sun.ResourceHub.Lock)
					{
						var r = Program.Sun.ResourceHub.Add(a);
						r.AddData(h);
					}

					Program.Api<byte[]>(new ReleaseDownloadCall {Release = h});

					try
					{
						ReleaseDownloadProgress d = null;
						
						while(Workflow.Active)
						{
							d = Program.Api<ReleaseDownloadProgress>(new ReleaseDownloadProgressCall {Release = h});

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
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
