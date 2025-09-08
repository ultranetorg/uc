using System.Reflection;

namespace Uccs.Fair;

public class NodeCommand : Uccs.Net.NodeCommand
{
	public NodeCommand(McvCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	public CommandAction Run()
	{
 		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "r";
		a.Help = new() {Description = "Runs a new instance with command-line interface",
						Syntax = $"{Keyword} {a.NamesSyntax} flags [profile={DIRPATH}]",

						Arguments =	[
										new ("name", "An arbitrary name of a node"),
										new ("profile", "Path to local profile directory"),
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} profile=C:\\NodeProfile")
									]};

		a.Execute = () =>	{
								Cli.Node = new FairNode(GetString("name", null), Cli.Net.Zone, Cli.Settings.Profile, Cli.Settings as FairNodeSettings, new RealClock(), Flow);

								if(ConsoleAvailable)
								{
									while(Flow.Active)
									{
										Console.Write($"{Cli.Node.Net.Address} >");

										try
										{
											var x = new Xon(Console.ReadLine());

											Cli.LogView.StartListening(Flow.Log);
											
											if(x.Nodes[0].Name == Keyword && (
																				a.Names.Contains(x.Nodes[1].Name) 
																			 ))
												throw new Exception("Not available");

											Cli.Execute(x.Nodes, Flow);
											Cli.LogView.StopListening();
										}
										catch(Exception ex)
										{
											Flow.Log.ReportError(this, "Error", ex);
										}
									}

								}
								else
									WaitHandle.WaitAny([Flow.Cancellation.WaitHandle]);

								Cli.Node.Stop();

								return null;
							};
		
		return a;
	}
}

