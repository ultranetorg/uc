﻿namespace Uccs.Rdn.CLI
{
	/// <summary>
	/// </summary>
	public class NexusCommand : RdnCommand
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
									Syntax = "nexus m|membership UAA",

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
													Flow.CancelAfter(program.Settings.RdcQueryTimeout);

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