﻿namespace Uccs.Net;

public class NodeCommand : McvCommand
{
	public const string Keyword = "node";

	public NodeCommand(McvCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
		//var run		= new CommandAction {Names = ["r", "run"]};
		var attach	= new CommandAction {Names = ["a", "attach"] };
		var send	= new CommandAction {Names = ["s", "send"]};

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
			
		attach.Execute = () =>	{
									ReportPreambule();
									ReportNetwork();
										
									var a = new Uri(Args[0].Name);

									var h = new HttpClientHandler();
									h.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
									var http = new HttpClient(h){Timeout = TimeSpan.FromSeconds(60)};

									Cli.ApiClient = new ApiClient(http, Args[0].Name, GetString("accesskey", null));

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
												||  x.Nodes[0].Name == LogCommand.Keyword)
											{ 
												throw new Exception("Not available");
											}
	
											Cli.Execute(x.Nodes, flow);
										}
										catch(Exception ex)
										{
											Flow.Log?.ReportError(this, "Error", ex);
										}
									}

									return null;

								};

		attach.Help = new Help{ Title = "Attach",
								Description = "Connects to existing node instance via JSON RPC protocol",
								Syntax = $"{Keyword} {attach.NamesSyntax} HOST accesskey=PASSWORD",

								Arguments =
								[
									new ("<first>", "URL address of node to connect to"),
									new ("accesskey", "API access key")
								],

								Examples =
								[
									new (null, $"{Keyword} {attach.Names[0]} 127.0.0.1:3901 asscesskey=ApiServerSecret")
								]};


		send.Execute = () => {
								ReportPreambule();
								ReportNetwork();

								var a = new Uri(Args[0].Name);

								var h = new HttpClientHandler();
								h.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
								var http = new HttpClient(h){Timeout = TimeSpan.FromSeconds(60)};

								Cli.ApiClient = new ApiClient(http, Args[0].Name, GetString("accesskey", null));

								if(Has("_confirmation"))
								{
									Console.WriteLine("_confirmation reqested. Press any key...");
									Console.ReadKey();
								}

								Cli.Execute(Args.Skip(1).Where(i => i.Name != "accesskey" && i.Name != "_confirmation"), flow);

								if(Has("_confirmation"))
								{
									Console.WriteLine("_confirmation reqested. Press any key...");
									Console.ReadKey();
								}

								return null;
							};

		send.Help = new Help {	Title = "Send",
								Description = "Send spicified command to existing running node",
								Syntax = $"{Keyword} {send.NamesSyntax} HOST accesskey=PASSWORD command",

								Arguments =
								[
									new ("HOST", "URL address of node to send a command to"),
									new ("accesskey", "API access key"),
									new ("command", "A command to send for execution")
								],

								Examples =
								[
									new (null,$"{Keyword} {send.Names[0]} 127.0.0.1:3901 asscesskey=MyApiServerSecret node peers")
								]
							};

		var peers = new CommandAction{	Names = ["peers"],
										Execute = () =>	{
															var r = Api<PeersReportApc.Return>(new PeersReportApc {Limit = int.MaxValue});
																
															Dump(	r.Peers, 
																	["IP", "Status", "PeerRank", "Roles"], 
																	[i => i.IP, i => i.Status, i => i.PeerRank, i => i.Roles]);
													
															return r;
														}};

		peers.Help = new () {	Title = "Peers",
								Description = "Gets a list of existing connections",
								Syntax = $"{Keyword} {peers.NamesSyntax}",

								Arguments = [],

								Examples =
								[
									new (null, "node peers")
								]};

		var it = new CommandAction{	Names = ["it", "incomingtransactions"],
									Execute = () =>	{
														var r = Api<ApcTransaction[]>(new IncomingTransactionsApc{});
			
														foreach(var t in r)
														{
															Dump(t);

															foreach(var o in t.Operations)
																Report("   " + o.Description);
														}
													
														return r;
													}};

		it.Help = new Help{	Title = "Incoming Transactions",
							Description = "Gets current list of incomming transactions",
							Syntax = $"{Keyword} {it.NamesSyntax}",

							Arguments = [],

							Examples =
							[
								new (null, $"{Keyword} {it.Names[0]}")
							]};

		var ot = new CommandAction{	Names = ["ot", "outgoingtransactions"],
									Execute = () =>	{
														var r = Api<ApcTransaction[]>(new OutgoingTransactionsApc{});
			
														foreach(var t in r)
														{
															Dump(t);

															foreach(var o in t.Operations)
																Report("   " + o.Description);
														}
													
														return r;
													}};

		ot.Help = new Help{ Title = "Outgoing Transactions",
							Description = "Gets current list of outgoing transactions",
							Syntax = $"{Keyword} {ot.NamesSyntax}",

							Arguments = [],

							Examples =
							[
								new (null, $"{Keyword} {ot.Names[0]}")
							]};

		var property = new CommandAction{	Names = ["property"],
											Execute = () =>	{
																var r = Api<string>(new PropertyApc {Path = Args[0].Name});
			
																Report(r);
					
																return r;
															}};


		property.Help = new Help {	Title = "Property",
									Description = "Displays a value of node internal state",
									Syntax = $"{Keyword} {property.NamesSyntax} TEXT",

									Arguments = [],

									Examples =
									[
										new (null, "node property Mcv.Size")
									]};
			
		var mebbership = new CommandAction(){
												Names = ["m", "membership"],

												Help = new Help
												{ 
													Title = "Membership",
													Description = "Get information about membership status of specified account",
													Syntax = "nexus m|membership UAA",

													Arguments =
													[
														new ("<first>>", "Ultranet account public address to check the membership status")
													],

													Examples =
													[
														new (null, $"{Keyword} m 0x0000fffb3f90771533b1739480987cee9f08d754")
													]
												},

												Execute = () =>	{
																	Flow.CancelAfter(cli.Settings.RdcQueryTimeout);

																	var rp = Rdc(new MembersRequest());
	
																	var m = rp.Members.FirstOrDefault(i => i.Address == AccountAddress.Parse(Args[0].Name));

																	if(m == null)
																		throw new EntityException(EntityError.NotFound);

																	Dump(m);

																	return m;
																}
											};


		Actions = [attach, send, mebbership, peers, it, ot, property];
		
	}

}

