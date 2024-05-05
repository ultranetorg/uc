using System;
using System.Collections.Generic;
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

		public ReleaseCommand(Program program, List<Xon> args) : base(program, args)
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
					if(!Has("source") && !Has("sources"))
						throw new SyntaxException("Unknown arguments");

					var a = Api<Urr>(new ReleaseBuildApc {	Source = GetString("source", null),
															Sources = GetString("sources", null)?.Split(','),
															AddressCreator = new()	{	
																						Type = GetEnum("addresstype", UrrScheme.Urrh),
																						Owner = GetAccountAddress("owner", false),
																						Resource = Ura.Parse(Args[1].Name)
																					} });

					Report($"Address   : {a}");

					return a;
				}

				case "l" : 
				case "local" : 
				{	
					var r = Api<LocalReleaseApc.Release>(new LocalReleaseApc {Address = Urr.Parse(Args[1].Name)});
					
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
