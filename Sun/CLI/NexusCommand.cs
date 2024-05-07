using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// </summary>
	public class NexusCommand : Command
	{
		public const string Keyword = "nexus";

		public NexusCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{
			Actions =	[
							new ()
							{
								Names = ["m", "membership"],

								Help = new Help
								{ 
									Title = "MEMBERSHIP",
									Description = "Get information about membership status of specified account",
									Syntax = "nexus m|membership ACCOUNT",

									Arguments =
									[
										new ("<first>>", "Ultranet account public address to check the membership status")
									],

									Examples =
									[
										new (null, "nexus m 0x0000fffb3f90771533b1739480987cee9f08d754")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcQueryTimeout);

													var rp = Rdc(new MembersRequest());
	
													var m = rp.Members.FirstOrDefault(i => i.Account == AccountAddress.Parse(Args[0].Name));

													if(m == null)
														throw new EntityException(EntityError.NotFound);

													Dump(m);

													return m;
												}
							},
						];
		}
	}
}
