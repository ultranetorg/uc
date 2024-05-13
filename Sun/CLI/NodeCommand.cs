using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class NodeCommand : Command
	{
		public const string Keyword = "node";

		public NodeCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{
			Actions =	[
							new ()
							{
								Names = ["r", "run"],

								Help = new Help
								{ 
									Title = "RUN",
									Description = "Runs a new node instance with command-line interface",
									Syntax = "node r|run flags [profile=PATH] [zone=ZONE]",

									Arguments =
									[
										new ("flags", "One or more flags: 'api' to start JSON API Server, 'peer' to connect to Ultranet network and activate specified node roles, 'base' to enable Base support for the node database, 'chain' to enable Chain support for the node database, 'seed' to enable seed role for the node."),
										new ("profile", "File path to local profile directory"),
										new ("zone", "Network zone to connect")
									],

									Examples =
									[
										new (null, "node run api peer chain seed profile=C:\\User\\sun zone=Testzone1")
									]
								},

								Execute = () =>	{
													Program.Sun = new Net.Sun(Program.Zone, Program.Settings, Flow){Clock = new RealClock(),
																													Nas = new Nas(Program.Settings),
																													GasAsker = ConsoleAvailable ? new ConsoleGasAsker() : new SilentGasAsker(),
																													FeeAsker = new SilentFeeAsker() };
													Program.Sun.Run(Args);
	
													if(ConsoleAvailable)
														while(Flow.Active)
														{
															Console.Write(Program.Zone + " > ");
	
															var l = new Log();
															var v = new ConsoleLogView(false, true);
															v.StartListening(l);
	
															try
															{
																var x = new XonDocument(Console.ReadLine());
	
																if(x.Nodes[0].Name == Keyword && (	x.Nodes[1].Name == "run" || x.Nodes[1].Name == "attach" || x.Nodes[1].Name == "send" ||
																									x.Nodes[1].Name == "r" || x.Nodes[1].Name == "a" || x.Nodes[1].Name == "s"
																									))
																	throw new Exception("Command not available");
	
																Program.Execute(x.Nodes, Flow);
															}
															catch(Exception ex)
															{
																l.ReportError(this, "Error", ex);
															}
	
															v.StopListening(l);
														}
													else
														WaitHandle.WaitAny([Flow.Cancellation.WaitHandle]);

													return null;
												}
							},

							new ()
							{
								Names = ["a", "attach"],

								Help = new Help
								{ 
									Title = "ATTACH",
									Description = "Connects to existing node instance via JSON RPC protocol",
									Syntax = "node a|attach HOST accesskey=PASSWORD",

									Arguments =
									[
										new ("<first>", "URL address of node to connect to"),
										new ("accesskey", "API access key")
									],

									Examples =
									[
										new (null, "node attach 127.0.0.1:3901 asscesskey=ApiServerSecret")
									]
								},

								Execute = () =>	{
													var a = new Uri(Args[0].Name);

													var h = new HttpClientHandler();
													h.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
													var http = new HttpClient(h){Timeout = TimeSpan.FromSeconds(60)};

													Program.ApiClient = new SunJsonApiClient(http, Args[0].Name, GetString("accesskey", null));

													var v = new ConsoleLogView(false, true);
													v.StartListening(Flow.Log);

													while(true)
													{
														Console.Write($"{a.Host}:{a.Port} > ");
														var c = Console.ReadLine();

														if(c == "exit")
															break;

														try
														{
															var x = new XonDocument(c);

															if(x.Nodes[0].Name == Keyword || x.Nodes[0].Name == LogCommand.Keyword)
																throw new Exception("Not available");
	
															Program.Execute(x.Nodes, flow);
														}
														catch(Exception ex)
														{
															Flow.Log?.ReportError(this, "Error", ex);
														}
													}

													v.StopListening(Flow.Log);

													return null;

												}
							},

							new ()
							{
								Names = ["s", "send"],

								Help = new Help
								{
									Title = "SEND",
									Description = "Send spicified command to existing running node",
									Syntax = "node s|send HOST accesskey=PASSWORD command",

									Arguments =
									[
										new ("HOST", "URL address of node to send a command to"),
										new ("accesskey", "API access key"),
										new ("command", "A command to send for execution")
									],

									Examples =
									[
										new (null, "node send 127.0.0.1:3901 asscesskey=ApiServerSecret node peers")
									]
								},

								Execute = () =>	{
													var a = new Uri(Args[0].Name);

													var h = new HttpClientHandler();
													h.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
													var http = new HttpClient(h){Timeout = TimeSpan.FromSeconds(60)};

													Program.ApiClient = new SunJsonApiClient(http, Args[0].Name, GetString("accesskey", null));

													var v = new ConsoleLogView(false, true);
													v.StartListening(Flow.Log);

													try
													{
														Program.Execute(Args.Skip(1).Where(i => i.Name != "accesskey"), flow);
													}
													finally
													{
														v.StopListening(Flow.Log);
													}

													return null;
												}
							},

							new ()
							{
								Names = ["peers"],

								Help = new Help
								{ 
									Title = "PEERS",
									Description = "Gets a list of existing connections",
									Syntax = "node peers",

									Arguments =
									[
									],

									Examples =
									[
										new (null, "node peers")
									]
								},

								Execute = () =>	{
													var r = Api<PeersReport>(new PeersReportApc {Limit = int.MaxValue});
			
													Dump(	r.Peers, 
															["IP", "Status", "PeerRank", "BaseRank", "ChainRank", "SeedRank"], 
															[i => i.IP, i => i.Status, i => i.PeerRank, i => i.BaseRank, i => i.ChainRank, i => i.SeedRank]);
													
													return r;
												}
							},

							new ()
							{
								Names = ["property"],

								Help = new Help
								{ 
									Title = "PROPERTY",
									Description = "Displays a value of node internal state",
									Syntax = "node Mcv.Size",

									Arguments =
									[
									],

									Examples =
									[
										new (null, "node property")
									]
								},

								Execute = () =>	{
													var r = Api<string>(new PropertyApc {Path = Args[0].Name});
			
													Report(r);
					
													return r;
												}
							},
						];
		}
	}
}
