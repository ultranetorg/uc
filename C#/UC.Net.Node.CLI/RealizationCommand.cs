using System;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace UC.Net.Node.CLI
{
	/// <summary>
	/// Usage: realization register
	///							by = ACCOUNT 
	///							[password = PASSWORD] 
	///							product = PRODUCT 
	///							obis = OBIs - {windows.10.*.x64}
	/// </summary>
	public class RealizationCommand : Command
	{
		public const string Keyword = "realization";

		public RealizationCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "register" : 
					var c = new RealizationRegistration
							{ 
								Signer = GetPrivate("by", "password"), 
								Address = RealizationAddress.Parse(GetString("address")),
								OSes = Args.One("oses").Nodes.Select(i => OsBinaryIdentifier.Parse(i.Name)).ToArray()
							};

					return Send(() => Node.Enqueue(c));
		
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
