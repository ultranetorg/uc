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

					var h = Api<byte[]>(new ReleaseBuildCall{Resource = ResourceAddress.Parse(Args.Nodes[1].Name),
																	 FilePath = GetString("source", null),
																	 Sources = GetString("sources", null)?.Split(',')});

					Workflow.Log?.Report(this, $"Hash={h.ToHex()}");

					return null;
				}

				case "d" :
				case "download" :
				{
					var h = Args.Nodes[1].Name.FromHex();

					Api<byte[]>(new ReleaseDownloadCall {Release = h, Type = Enum.Parse<DataType>(GetString("type")) });

					try
					{
						ReleaseDownloadProgress d = null;
						
						while(Workflow.Active)
						{
							d = Api<ReleaseDownloadProgress>(new ReleaseDownloadProgressCall {Release = h});

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
					var r = Api<LocalReleaseCall.Release>(new LocalReleaseCall {Address = Args.Nodes[1].Name.FromHex()});
					
					if(r != null)
					{
						Dump(r);
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
