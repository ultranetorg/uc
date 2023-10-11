using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		
	/// </summary>
	///  <example>
	///		package download _aaa/app/win32/0.0.5
	///  </example>
	public class PackageCommand : Command
	{
		public const string Keyword = "package";

		PackageAddress Package => PackageAddress.Parse(Args.Nodes[1].Name);

		public PackageCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "build" :
				{
					Program.Call(new PackageBuildCall {	Package	= Package, 
														Version = Args.Has("previous") ? Version.Parse(GetString("previous")) : null,
														Sources = GetString("sources").Split(','), 
														DependsDirectory =GetString("dependsdirectory") });
					return null;
				}

				case "download" :
				{
					var h = Program.Call<byte[]>(new PackageDownloadCall {Package = Package});

					try
					{
						PackageDownloadProgress d;
						
						do
						{
							d = Program.Call<PackageDownloadProgress>(new PackageDownloadProgressCall {Package = Package});
							
							if(d == null)
							{	
								if(!Program.Call<bool>(new PackageReadyCall {Package = Package}))
								{
									Workflow.Log?.ReportError(this, "Failed");
								}

								break;
							}

							Workflow.Log?.Report(this, d.ToString());

							Thread.Sleep(500);
						}
						while(d != null && Workflow.Active);
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
