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
	public class ReleaseCommand : Command
	{
		public const string Keyword = "release";

		public ReleaseCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "b" :
				case "build" :
				{
					if(!Args.Has("source") && !Args.Has("sources"))
						throw new SyntaxException("Unknown arguments");

					var h = Program.Api<byte[]>(new ReleaseBuildCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name),
																		FilePath = GetString("source", null),
																		Sources = GetString("sources", null)?.Split(',')});

					Workflow.Log?.Report(this, $"Hash={h.ToHex()}");

					return null;
				}

				case "d" :
				case "download" :
				{
					var a = ResourceAddress.Parse(Args.Nodes[1].Name);
		
					var h = Args.Has("hash") ? Args.Get<string>("hash").HexToByteArray() : Program.Api<byte[]>(new ResourceDownloadCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});

					try
					{
						ResourceDownloadProgress d = null;
						
						while(Workflow.Active)
						{
							d = Program.Api<ResourceDownloadProgress>(new ResourceDownloadProgressCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name), Hash = h});

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

				case "l" : 
				case "local" : 
				{	
					var r = Program.Api<IEnumerable<LocalRelease>>(new LocalReleasesCall {Resource = ResourceAddress.Parse(Args.Nodes[1].Name)});
					
					Dump(	r, 
							new string[] {"Hash", "Type", "Availability"}, 
							new Func<LocalRelease, string>[]{	i => i.Hash.ToHex(),
																i => i.Type.ToString(),
																i => i.Availability.ToString() });

					return r;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
