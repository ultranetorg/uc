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
		PackageAddress		Package => PackageAddress.Parse(Args.Nodes[1].Name);

		public PackageCommand(Program program, Xon args) : base(program, args)
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
					Api(new PackageBuildCall {	Resource		 = ResourceAddress.Parse(Args.Nodes[1].Name), 
												Sources			 = GetString("sources").Split(','), 
												DependenciesPath = GetString("dependencies", false),
												Previous		 = GetHexBytes("previous", false) });
					return null;
				}

				case "l" :
				case "local" :
				{
					var r = Api<PackageInfo>(new PackageInfoCall {Package = Package});
					
					Dump(r);

					return null;
				}

				case "d" :
				case "download" :
				{
					var h = Api<byte[]>(new PackageDownloadCall {Package = Package});

					try
					{
						PackageDownloadProgress d;
						
						do
						{
							d = Api<PackageDownloadProgress>(new PackageActivityProgressCall {Package = Package});
							
							if(d == null)
							{	
								if(!Api<PackageInfo>(new PackageInfoCall {Package = Package}).Ready)
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

				case "i" :
				case "install" :
				{
					Api(new PackageInstallCall {Package = Package});

					try
					{
						ResourceActivityProgress d = null;
						
						do
						{
							d = Api<ResourceActivityProgress>(new PackageActivityProgressCall {Package = Package});
							
							if(d == null)
							{	
								if(!Api<PackageInfo>(new PackageInfoCall {Package = Package}).Ready)
									Workflow.Log?.ReportError(this, "Failed");
								
								break;
							}
							else
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
