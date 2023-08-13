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

		public ResourceCommand(Zone zone, Settings settings, Log log, Func<Net.Sun> sun, Xon args) : base(zone, settings, log, sun, args)
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
					var r =	new ResourceCreation(	GetPrivate("by", "password"), 
													ResourceAddress.Parse(GetString("address")),
													byte.Parse(GetString("years")),
													Args.Has("flags")		? Enum.Parse<ResourceFlags>(GetString("flags")) : ResourceFlags.Null,
													Args.Has("type")		? Enum.Parse<ResourceType>(GetString("type")) : ResourceType.Null,
													Args.Has("data")		? GetHexBytes("data") : null,
													Args.Has("parent")		? GetString("parent") : null, 
													Args.Has("analysisfee") ? Coin.ParseDecimal(Args.GetString("analysisfee")) : Coin.Zero);

					return Sun.Enqueue(r, GetAwaitStage(), Workflow);
				}

				case "u" : 
				case "update" : 
				{	
					var r =	new ResourceUpdation(GetPrivate("by", "password"), ResourceAddress.Parse(GetString("address")));

					if(Args.Has("years"))		r.Change(byte.Parse(GetString("years")));
					if(Args.Has("flags"))		r.Change(Enum.Parse<ResourceFlags>(GetString("flags")));
					if(Args.Has("type"))		r.Change(Enum.Parse<ResourceType>(GetString("type")));
					if(Args.Has("data"))		r.Change(GetHexBytes("data"));
					if(Args.Has("parent"))		r.Change(GetString("parent"));
					if(Args.Has("analysisfee")) r.Change(Coin.ParseDecimal(Args.GetString("analysisfee")));
					if(Args.Has("recursive"))	r.ChangeRecursive();
					
					return Sun.Enqueue(r, GetAwaitStage(), Workflow);
				}

				case "s" :
		   		case "status" :
				{
					try
					{
						var r = Sun.Call(Role.Base, i => i.FindResource(GetResourceAddress("address")), Workflow);
	
						Dump(r.Resource);
						//Workflow.Log?.Report(this, GetString("address"));
						//Workflow.Log?.Report(this, "   " + r.Resource.Flags);
						//Workflow.Log?.Report(this, "   " + r.Resource.Type);
						//Workflow.Log?.Report(this, "   " + r.Resource.Expiration);
						//
						//if(r.Resource.Data != null)
						//	Workflow.Log?.Report(this, "   " + Hex.ToHexString(r.Resource.Data));

						var e = Sun.Call(Role.Base, i => i.EnumerateSubresources(GetResourceAddress("address")), Workflow);

						if(e.Resources.Any())
						{
							//Workflow.Log?.Report(this, "   Subresources:");

							foreach(var i in e.Resources)
							{
								Workflow.Log?.Report(this, "      " + i);
							}
						}

						return r;
					}
					catch(RdcEntityException ex)
					{
						Workflow.Log?.Report(this, ex.Message);
						return null;
					}
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
