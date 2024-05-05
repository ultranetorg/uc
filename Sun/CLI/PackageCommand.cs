using System;
using System.Collections.Generic;
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

	public class PackageCommand : Command
	{
		public const string Keyword = "package";
		Ura		Package => Ura.Parse(Args[1].Name);

		public PackageCommand(Program program, List<Xon> args) : base(program, args)
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
					Ura p = null;
					Manifest m = null;

					try
					{
						p = GetResourceAddress("previous", false);
							//: Rdc<ResourceResponse>(new ResourceRequest {Resource = ResourceAddress.Parse(Args[1].Name)}).Resource.Data?.Interpretation as ReleaseAddress;
						
						if(p != null)
						{
							m = Api<PackageInfo>(new PackageInfoApc {Package = p}).Manifest;
						}
					}
					catch(EntityException ex) when(ex.Error == EntityError.NotFound)
					{
					}

					Api(new PackageBuildApc {	Resource		 = Ura.Parse(Args[1].Name), 
												Sources			 = GetString("sources").Split(','), 
												DependenciesPath = GetString("dependencies", false),
												Previous		 = p,
												History			 = m?.History,
												AddressCreator	 = new(){	
																			Type = GetEnum("addresstype", UrrScheme.Urrh),
																			Owner = GetAccountAddress("owner", false),
																			Resource = Ura.Parse(Args[1].Name)
																		} });
					return null;
				}

				case "l" :
				case "local" :
				{
					var r = Api<PackageInfo>(new PackageInfoApc {Package = Package});
					
					Dump(r);

					return null;
				}

				case "d" :
				case "download" :
				{
					var h = Api<byte[]>(new PackageDownloadApc {Package = Package});

					try
					{
						PackageDownloadProgress d;
						
						do
						{
							d = Api<PackageDownloadProgress>(new PackageActivityProgressApc {Package = Package});
							
							if(d == null)
							{	
								if(!Api<PackageInfo>(new PackageInfoApc {Package = Package}).Ready)
								{
									Workflow.Log?.ReportError(this, "Failed");
								}

								break;
							}

							Report(d.ToString());

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
					Api(new PackageInstallApc {Package = Package});

					try
					{
						ResourceActivityProgress d = null;
						
						do
						{
							d = Api<ResourceActivityProgress>(new PackageActivityProgressApc {Package = Package});
							
							if(d == null)
							{	
								if(!Api<PackageInfo>(new PackageInfoApc {Package = Package}).Ready)
									Workflow.Log?.ReportError(this, "Failed");
								
								break;
							}
							else
								Report(d.ToString());

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
