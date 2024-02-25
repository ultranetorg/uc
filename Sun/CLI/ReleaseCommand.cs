using System;
using System.Linq;
using System.Threading;
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
				case "c" :
				case "create" :
				{
					if(!Args.Has("source") && !Args.Has("sources"))
						throw new SyntaxException("Unknown arguments");

					var a = Api<ReleaseAddress>(new ReleaseBuildCall{Resource = ResourceAddress.Parse(Args.Nodes[1].Name),
																	 FilePath = GetString("source", null),
																	 Sources = GetString("sources", null)?.Split(','),
																	 AddressCreator = new()	{	
																								Type = GetEnum<ReleaseAddressType>("addresstype", ReleaseAddressType.DH),
																								Owner = GetAccountAddress("owner", false),
																								Resource = ResourceAddress.Parse(Args.Nodes[1].Name)
																							} });

					Workflow.Log?.Report(this, $"Address : {a}");

					return null;
				}

				case "d" :
				case "download" :
				{
					var a = ReleaseAddress.Parse(Args.Nodes[1].Name);

					Api(new ReleaseDownloadCall {Address = a, Type = Enum.Parse<DataType>(GetString("type")) });

					try
					{
						ReleaseDownloadProgress d = null;
						
						while(Workflow.Active)
						{
							d = Api<ReleaseDownloadProgress>(new ReleaseActivityProgressCall {Release = a});

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

 				case "e" :
 		   		case "entity" :
 				{
 					Workflow.CancelAfter(RdcQueryTimeout);
 
 					var r = Rdc<ReleaseResponse>(new ReleaseRequest {Release = ReleaseAddress.Parse(Args.Nodes[1].Name)});
 					
 					Dump(r.Release);
 
 					return r;
 				}

				case "l" : 
				case "local" : 
				{	
					var r = Api<LocalReleaseCall.Release>(new LocalReleaseCall {Address = ReleaseAddress.Parse(Args.Nodes[1].Name)});
					
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
