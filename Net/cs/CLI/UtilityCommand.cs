using System.Reflection;

namespace Uccs.Net;

public class UtilityCommand : McvCommand
{
	public UtilityCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Transfer()
	{
		var from = "from";
		var to = "to";

		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "t";

		a.Help = new() {Description = "Send utility from one account to another.",
						Syntax = $"{Keyword} {a.NamesSyntax} {from}={ET}/{EID} {to}={ET}/{EID} st={ST}|e={EC}|en={EC} {SignerArg}={AA}",

						Arguments =	[new (from, "Entity type and id where funds are credited from"),
									 new (to, "Entity type and id where funds are credited to"),
									 new ("st", "Amount of space-time to be transferred"),
									 new ("ec", "Amount of Execution Cycles of the currect year to be transferred"),
									 new ("ecnext", "Amount of Execution Cycles of next year to be transferred"),
									 new (SignerArg, "Account public address where funds are debited from")],

						Examples =	[new (null, $"{Keyword} {a.Name} {from}={ET.Example}/{EID.Example[1]} {to}={ET.Example1}/{EID.Example[2]} e=1.5 {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var info = Ppc(new InfoRequest {});
								
								var toe = GetString(to).Split('/')[1];

								AutoId toid;

								if(toe == "lastcreated")
									toid = AutoId.LastCreated;
								else
									toid = AutoId.Parse(toe);

								return new UtilityTransfer( info.Tables[GetString(from).Split('/')[0]], 
															AutoId.Parse(GetString(from).Split('/')[1]),
															info.Tables[GetString(to).Split('/')[0]], 
															toid,
															GetEC("e", 0), 
															GetEC("en", 0), 
															GetBD("st", 0));
							};

		return a;
	}
}
