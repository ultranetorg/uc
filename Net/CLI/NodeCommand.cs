using System.Reflection;

namespace Uccs.Net;

public abstract class NodeCommand : McvCommand
{
	CommandAction attach;
	CommandAction send;

	protected abstract McvApiClient		CreateClient(string url);

	public NodeCommand(McvCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	public CommandAction Attach_a()
	{
		attach = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		attach.Execute = () =>	{
									ReportPreambule();
									ReportNetwork();
										
									var a = new Uri(First);

									Cli.ApiClient = CreateClient(First);

									while(true)
									{
										Console.Write($"{a.Host}:{a.Port} > ");
										var c = Console.ReadLine();

										if(c == "exit")
											break;

										var x = new Xon(c);

										if((First == Keyword && (attach.Names.Contains(x.Nodes[1].Name) || 
																 //run.Names.Contains(x.Nodes[1].Name)  || 
																 send.Names.Contains(x.Nodes[1].Name)))
											|| First == new LogCommand(null, null, null).Keyword)
										{ 
											Console.WriteLine("Not available");
										}
	
										Cli.Execute(Cli.Boot.Profile, x);
									}

									return null;
								};

		attach.Description = "Connects to existing node instance via JSON RPC protocol";
		attach.Arguments =	[
								new (AddressKeyword, URL, "URL address of node to connect to"),
								new (Apc.CredentialsKeyword, PASSWORD, "API access key")
							];

		return attach;
	}


	public CommandAction Send_s()
	{
		send = new CommandAction(this, MethodBase.GetCurrentMethod());

		send.Description = "Send specified command to existing running node";
		send.Arguments =	[
								new (AddressKeyword,URL, "HOST address of node to send a command to"),
								new (Apc.CredentialsKeyword, PASSWORD, "API access key", ArgumentFlag.Optional),
								new ("command",		COMMAND, "A command to send for execution")
							];

		send.Execute = () => {
								ReportPreambule();
								ReportNetwork();

								var a = new Uri(First);

								Cli.ApiClient = CreateClient(First);

								Cli.Execute(Args.Skip(1).Where(i => new string[] {Apc.CredentialsKeyword, ConfirmationArg}.All(j => j != i.Name)), Flow);

								return null;
							};

		return send;
	}

	public CommandAction Peers()
	{ 
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Execute = () =>	{
								var r = Api<PeersReportApc.Return>(new PeersReportApc {Limit = int.MaxValue});
																
								Flow.Log.Dump(	r.Peers, 
												["IP",		"Status",		"PeerRank",		 "Roles"], 
												[i => i.EP, i => i.Status,	i => i.PeerRank, i => i.Roles]);
													
								return r;
							};

		a.Description = "Gets the list of existing connections";

		return a;
	}

	public CommandAction IncomingTransactions_it()
	{ 
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Description = "Applicable when node is a member of consensus. Gets current list of transactions that is going to be added to blocks by this node or are waiting for confirmation by the network";
		a.Execute = () =>	{
								var r = Api<TransactionApe[]>(new IncomingTransactionsApc{});
			
								foreach(var t in r)
								{
									Flow.Log.Dump(t);

									foreach(var o in t.Operations)
										Report("   " + o.Explanation);
								}
													
								return r;
							};


		return a;
	}

	public CommandAction OutgoingTransactions_ot()
	{ 
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Description = "Gets current list of transactions that are going to be sent to or are waiting for confirmation by the network";
		a.Execute = () =>	{
								var r = Api<TransactionApe[]>(new OutgoingTransactionsApc{});
			
								foreach(var t in r)
								{
									Flow.Log.Dump(t);

									foreach(var o in t.Operations)
										Report("   " + o.Explanation);
								}
													
								return r;
							};


		return a;
	}

	//public CommandAction Property()
	//{ 
	//	var a = new CommandAction(this, MethodBase.GetCurrentMethod());
	//	
	//	a.Name = "property";
	//	a.Description = "Displays a value of node internal state",
	//					Syntax = $"{Keyword} {a.NamesSyntax} {TEXT}",
	//
	//					a.Arguments = [new (null, PROPER )],
	//					};
	//
	//	a.Execute = () =>	{
	//							var r = Api<string>(new PropertyApc {Path = Args[0].Name});
	//		
	//							Report(r);
	//				
	//							return r;
	//						};
	//	return a;
	//}
}

