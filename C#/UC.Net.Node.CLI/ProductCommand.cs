using System;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace UC.Net.Node.CLI
{
	/// <summary>
	/// Usage: product register 
	///							by = ACCOUNT 
	///							[password = PASSWORD] 
	///							under = AUTHOR 
	///							name = PRODUCT 
	///							title = TITLE
	///		   product publish 
	///							by = ACCOUNT 
	///							[password = PASSWORD]
	///							product = PRODUCT
	///							version = VERSION
	///							platform = PLATFORM
	///							manifest = MANIFEST
	/// </summary>
	public class ProductCommand : Command
	{
		public const string Keyword = "product";

		public ProductCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "register" : 
					return Send(() => Client.Enqueue(new ProductRegistration(	GetPrivate("by", "password"), 
																				ProductAddress.Parse(GetString("address")),
																				GetString("title"))));

				case "publish" : 
					return Send(() => Client.Enqueue(new ReleaseManifest (	GetPrivate("by", "password"), 
																			ReleaseAddress.Parse(GetString("address")),
																			GetString("channel"), 
																			Version.Parse(GetString("previous")),
																			GetStringOrEmpty("cdependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i =>  ReleaseAddress.Parse(i)).ToList(),
																			GetStringOrEmpty("idependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i =>  ReleaseAddress.Parse(i)).ToList()
																			)));

		   		case "releasestatus" :
				{
					var r = Client.QueryRelease(new []{ReleaseQuery.Parse("query")}, Args.Has("confirmed"));

					Log.Report(this, "Release", r.ToString());

					return r;
				}
				
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
