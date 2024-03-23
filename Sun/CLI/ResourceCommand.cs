using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
												GetEnum("flags", ResourceFlags.None),
												GetData());
				}

				case "cl" : 
				case "createlink" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new ResourceLinkCreation(ResourceAddress.Parse(Args.Nodes[1].Name), GetResourceAddress("to"));
				}

				case "u" : 
				case "update" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					var r =	new ResourceUpdation(ResourceAddress.Parse(Args.Nodes[1].Name));

					if(Has("flags"))		r.Change(GetEnum("flags", ResourceFlags.None));
					if(HasData())			r.Change(GetData());
					if(Has("recursive"))	r.ChangeRecursive();

					return r;
				}

				case "e" :
		   		case "entity" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);

					var r = Rdc<ResourceByNameResponse>(new ResourceByNameRequest {Name = ResourceAddress.Parse(Args.Nodes[1].Name)});
					
					Dump(r.Resource);

					return r;
				}

				case "lo" :
		   		case "listoutbounds" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);

					var r = Rdc<ResourceByNameResponse>(new ResourceByNameRequest {Name = ResourceAddress.Parse(Args.Nodes[1].Name)});
					
					Dump(	r.Resource.Links.Select(i => Rdc<ResourceByIdResponse>(new ResourceByIdRequest {ResourceId = i}).Resource), 
							["#", "Address", "Data"], 
							new Func<Resource, string>[]{	i => i.Id.ToString(),
															i => i.Address.ToString(),
															i => i.Data.Interpretation.ToString() });

					return r;
				}

				case "l" : 
				case "local" : 
				{	
					var r = Api<LocalResource>(new LocalResourceCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});
					
					if(r != null)
					{
						Dump(	r.Datas, 
								["Type", "Data", "Length"], 
								new Func<ResourceData, string>[] {	i => i.Type.ToString(), 
																	i => i.Value.ToHex(32), 
																	i => i.Value.Length.ToString() });
						return r;
					}
					else
						throw new Exception("Resource not found");
				}

				case "sl" : 
				case "searchlocal" : 
				{	
					Workflow.CancelAfter(RdcQueryTimeout);

					var r = Api<IEnumerable<LocalResource>>(new QueryLocalResourcesCall {Query = Args.Nodes[1].Name});
					
					Dump(	r, 
							["Address", "Releases", "Latest Type", "Latest Data", "Latest Length"], 
							new Func<LocalResource, string>[]{	i => i.Address.ToString(),
																i => i.Datas.Count.ToString(),
																i => i.Last.Type.ToString(),
																i => i.Last.Value.ToHex(32),
																i => i.Last.Value.Length.ToString() });
					return r;
				}

				case "d" :
				case "download" :
				{
					var a = ResourceAddress.Parse(Args.Nodes[1].Name);

					var r = Api<Resource>(new ResourceDownloadCall {Address = a});

					ReleaseDownloadProgress p = null;
						
					while(Workflow.Active)
					{
						p = Api<ReleaseDownloadProgress>(new ReleaseActivityProgressCall {Release = r.Data.Interpretation as ReleaseAddress});

						if(p == null)
							break;

						Workflow.Log?.Report(this, p.ToString());

						Thread.Sleep(500);
					}

					return null;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
