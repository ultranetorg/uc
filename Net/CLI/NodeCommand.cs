using System.Reflection;

namespace Uccs.Net;

public class NodeCommand : McvCommand
{
	CommandAction attach;
	CommandAction send;

	public NodeCommand(McvCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
		//var run		= new CommandAction {Names = ["r", "run"]};

// 			run.Execute = () =>	{
// 								};
// 
// 			run.Help = new Help(){	Title = "RUN",
// 									Description = "Runs a new node instance with command-line interface",
// 									Syntax = $"{Keyword} {run.NamesSyntax} flags [profile=PATH] [net=ZONE]",
// 
// 									Arguments =
// 									[
// 										new ("flags", "One or more flags: 'api' to start JSON API Server, 'peer' to connect to Ultranet network and activate specified node roles, 'base' to enable Base support for the node database, 'chain' to enable Chain support for the node database, 'seed' to enable seed role for the node."),
// 										new ("profile", "File path to local profile directory"),
// 										new ("net", "Network net to connect")
// 									],
// 
// 									Examples =
// 									[
// 										new (null, $"{Keyword} {run.Names[1]} api peer chain seed profile=C:\\User\\sun net=Testzone")
// 									]};
			
	}

	public CommandAction Attach()
	{
		attach = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		attach.Name = "a";
		attach.Execute = () =>	{
									ReportPreambule();
									ReportNetwork();
										
									var a = new Uri(Args[0].Name);

									var h = new HttpClientHandler();
									h.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
									var http = new HttpClient(h){Timeout = TimeSpan.FromSeconds(60)};

									Cli.ApiClient = new McvApiClient(Args[0].Name, GetString(Apc.AccessKey, null), http);

									while(true)
									{
										Console.Write($"{a.Host}:{a.Port} > ");
										var c = Console.ReadLine();

										if(c == "exit")
											break;

										try
										{
											var x = new Xon(c);

											if((x.Nodes[0].Name == Keyword && (attach.Names.Contains(x.Nodes[1].Name) || 
																				//run.Names.Contains(x.Nodes[1].Name)  || 
																				send.Names.Contains(x.Nodes[1].Name)))
												||  x.Nodes[0].Name == new LogCommand(null, null, null).Keyword)
											{ 
												throw new Exception("Not available");
											}
	
											Cli.Execute(x.Nodes, Flow);
										}
										catch(Exception ex)
										{
											Flow.Log?.ReportError(this, "Error", ex);
										}
									}

									return null;
								};

		attach.Description = "Connects to existing node instance via JSON RPC protocol";
		attach.Arguments =	[
								new (null, URL, "URL address of node to connect to"),
								new (Apc.AccessKey, PASSWORD, "API access key")
							];

		return attach;
	}


	public CommandAction Send()
	{
		send = new CommandAction(this, MethodBase.GetCurrentMethod());

		send.Name = "s";
		send.Execute = () => {
								ReportPreambule();
								ReportNetwork();

								var a = new Uri(Args[0].Name);

								var h = new HttpClientHandler();
								h.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
								var http = new HttpClient(h){Timeout = TimeSpan.FromSeconds(60)};

								Cli.ApiClient = new McvApiClient(Args[0].Name, GetString(Apc.AccessKey, null), http);

								if(Has("_confirmation"))
								{
									Console.WriteLine("_confirmation reqested. Press any key...");
									Console.ReadKey();
								}

								Cli.Execute(Args.Skip(1).Where(i => i.Name != Apc.AccessKey && i.Name != "_confirmation"), Flow);

								if(Has("_confirmation"))
								{
									Console.WriteLine("_confirmation requested. Press any key...");
									Console.ReadKey();
								}

								return null;
							};

		send.Description = "Send specified command to existing running node";
		send.Arguments =	[
								new (null,			URL, "HOST address of node to send a command to", Flag.First),
								new (Apc.AccessKey, PASSWORD, "API access key"),
								new ("command",		COMMAND, "A command to send for execution")
							];

		return send;
	}

	public CommandAction Peers()
	{ 
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Name = "peers";
		a.Execute = () =>	{
								var r = Api<PeersReportApc.Return>(new PeersReportApc {Limit = int.MaxValue});
																
								Flow.Log.Dump(	r.Peers, 
												["IP", "Status", "PeerRank", "Roles"], 
												[i => i.EP, i => i.Status, i => i.PeerRank, i => i.Roles]);
													
								return r;
							};

		a.Description = "Gets a list of existing connections";

		return a;
	}

	public CommandAction Incoming_Transactions()
	{ 
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Name = "it";
		a.Description = "Gets current list of incomming transactions";
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

	public CommandAction Outgoing_Transactions()
	{ 
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Name = "ot";
		a.Description = "Gets current list of outgoing transactions";
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
	
	public CommandAction Membership()
	{ 
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Name = "m";
		a.Description = "Get information about membership status of specified account";
		a.Arguments =	[
							new (null, AA, "Ultranet account public address to check the membership status", Flag.First)
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var rp = Ppc(new MembersPpc());
	
								var m = rp.Members.FirstOrDefault(i => i.Address == AccountAddress.Parse(Args[0].Name));

								if(m == null)
									throw new EntityException(EntityError.NotFound);

								Flow.Log.Dump(m);

								return m;
							};

		return a;
	}
}

