using System;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace UC.Net.Node.CLI
{
	/// <summary>
	/// Usage: product register 
	///							by = ACCOUNT 
	///							[password = PASSWORD] 
	///							address = AUTHOR/PRODUCT 
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
					return Core.Enqueue(new ProductRegistration(GetPrivate("by", "password"), 
																ProductAddress.Parse(GetString("address")),
																GetString("title")),
																GetAwaitStage(), 
																Workflow);

		
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
