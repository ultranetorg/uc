using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class ResourceCommand : Command
	{
		public const string Keyword = "package";

		public ResourceCommand(Zone zone, Settings settings, Log log, Func<Core> core, Xon args) : base(zone, settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "update" : 
					return Core.Enqueue(new ResourceUpdation(	GetPrivate("by", "password"), 
																ResourceAddress.Parse(GetString("address")),
																Args.Has("hash") ? GetHexBytes("hash") : null,
																Args.Has("flags") ? Enum.Parse<ResourceFlags>(GetString("flags")) : ResourceFlags.Null),
										GetAwaitStage(), 
										Workflow);

				
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
