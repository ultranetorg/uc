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
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new ResourceCreation(ResourceAddress.Parse(Args.Nodes[1].Name),
												Args.Has("flags")	? Enum.Parse<ResourceFlags>(GetString("flags")) : ResourceFlags.None,
												GetHexBytes("data", false),
												GetString("parent", false));
				}

				case "u" : 
				case "update" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

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
					Workflow.CancelAfter(RdcQueryTimeout);

					var r = Rdc<ResourceResponse>(new ResourceRequest {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});
					
					Dump(r.Resource);

					return r;
				}

				case "ls" : 
				case "list" : 
				{	
					Workflow.CancelAfter(RdcQueryTimeout);

					var r = Api<IEnumerable<LocalResource>>(new QueryLocalResourcesCall {Query = Args.Nodes[1].Name});
					
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
					var r = Api<LocalResource>(new LocalResourceCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});
					
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
					var d = Rdc<ResourceResponse>(new ResourceRequest {Resource = a}).Resource.Data;

					byte[] h;

					LocalResource lr;

					lock(Program.Sun.ResourceHub.Lock)
					{
						lr = Program.Sun.ResourceHub.Add(a);
						lr.AddData(d);
						h = lr.LastAs<byte[]>();
					}

					if(h == null)
						throw new Exception("Not supported type");

					Api<byte[]>(new ReleaseDownloadCall {Release = h, Type = lr.Last.Type});

					try
					{
						ReleaseDownloadProgress p = null;
						
						while(Workflow.Active)
						{
							p = Api<ReleaseDownloadProgress>(new ReleaseDownloadProgressCall {Release = h});

							if(p == null)
								break;

							Workflow.Log?.Report(this, p.ToString());

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
