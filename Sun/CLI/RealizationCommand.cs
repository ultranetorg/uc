﻿using System;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: realization register
	///							by = ACCOUNT 
	///							[password = PASSWORD] 
	///							product = PRODUCT 
	///							oses = OBIs - {windows.10.*.x64}
	/// </summary>
	public class PlatformCommand : Command
	{
		public const string Keyword = "platform";

		public PlatformCommand(Zone zone, Settings settings, Log log, Func<Core> core, Xon args) : base(zone, settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "register" : 
					return Core.Enqueue(new PlatformRegistration
										{ 
											Signer = GetPrivate("by", "password"), 
											Platform = RealizationAddress.Parse(GetString("address")),
											//OSes = Args.One("oses").Nodes.Select(i => Osbi.Parse(i.Name)).ToArray()
										},
										GetAwaitStage(), 
										Workflow);
		
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}