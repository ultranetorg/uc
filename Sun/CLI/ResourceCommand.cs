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

				case "i" :
		   		case "info" :
				{
					var r = Program.Api<Resource>(new ResourceEntityCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});

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

				case "l" : 
				case "local" : 
				{	
					var r = Program.Api<IEnumerable<LocalResource>>(new LocalResourcesCall {Query = Args.Nodes[1].Name});
					
					Dump(	r, 
							new string[] {"Address", "Releases", "Latest"}, 
							new Func<LocalResource, string>[]{	i => i.Address.ToString(),
																i => i.Datas.Count.ToString(),
																i => i.Last.ToHex() });

					return r;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
