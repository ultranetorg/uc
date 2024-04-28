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

		public ResourceCommand(Program program, List<Xon> args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.First().Name)
			{
				case "c" : 
				case "create" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new ResourceCreation(Ura.Parse(Args[1].Name),
												GetData(),
												Has("seal"));
				}

				case "x" : 
				case "destroy" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new ResourceDeletion(Ura.Parse(Args[1].Name));
				}

				case "u" : 
				case "update" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					var r =	new ResourceUpdation(Ura.Parse(Args[1].Name));

					if(HasData())			r.Change(GetData());
					if(Has("seal"))			r.Seal();
					if(Has("recursive"))	r.MakeRecursive();

					return r;
				}

				case "e" :
		   		case "entity" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);

					var r = Rdc(new ResourceByNameRequest {Name = Ura.Parse(Args[1].Name)});
					
					Dump(r.Resource);

					return r;
				}

				case "l" : 
				case "local" : 
				{	
					var r = Api<LocalResource>(new LocalResourceApc {Resource = Ura.Parse(Args[1].Name)});
					
					if(r != null)
					{
						Dump(	r.Datas, 
								["Type", "Data", "Length"], 
								[i => i.Type, i => i.Value.ToHex(32), i => i.Value.Length]);

						return r;
					}
					else
						throw new Exception("Resource not found");
				}

				case "sl" : 
				case "searchlocal" : 
				{	
					Workflow.CancelAfter(RdcQueryTimeout);

					var r = Api<IEnumerable<LocalResource>>(new QueryLocalResourcesApc {Query = Args[1].Name});
					
					Dump(	r, 
							["Address", "Releases", "Latest Type", "Latest Data", "Latest Length"], 
							[i => i.Address,
							 i => i.Datas.Count,
							 i => i.Last.Type,
							 i => i.Last.Value.ToHex(32),
							 i => i.Last.Value.Length]);
					return r;
				}

				case "d" :
				case "download" :
				{
					var a = Ura.Parse(Args[1].Name);

					var r = Api<Resource>(new ResourceDownloadApc {Address = a});

					ReleaseDownloadProgress p = null;
						
					while(Workflow.Active)
					{
						p = Api<ReleaseDownloadProgress>(new ReleaseActivityProgressApc {Release = r.Data.Interpretation as Urr});

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
