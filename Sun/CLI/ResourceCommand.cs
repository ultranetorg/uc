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
												Args.Has("type")	? Enum.Parse<ResourceType>(GetString("type")) : ResourceType.None,
												GetHexBytes("data", false),
												GetString("parent", false));
				}

				case "u" : 
				case "update" : 
				{	
					var r =	new ResourceUpdation(ResourceAddress.Parse(Args.Nodes[1].Name));

					if(Args.Has("flags"))		r.Change(Enum.Parse<ResourceFlags>(GetString("flags")));
					if(Args.Has("type"))		r.Change(Enum.Parse<ResourceType>(GetString("type")));
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
							new string[] {"Address", "Releases", "Latest", "Latest Length", }, 
							new Func<LocalResource, string>[]{	i => i.Address.ToString(),
																i => i.Datas.Count.ToString(),
																i => i.Last.ToHex(32),
																i => i.Last.Length.ToString()});
					return r;
				}

				case "l" : 
				case "local" : 
				{	
					var r = Program.Api<LocalResource>(new LocalResourceCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});
					
					if(r != null)
					{
						Dump(	r.Datas, 
								new string[] {"Data", "Length"}, 
								new Func<byte[], string>[] {i => i.ToHex(32), i => i.Length.ToString()});
						return r;
					}
					else
						throw new Exception("Resource not found");
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
