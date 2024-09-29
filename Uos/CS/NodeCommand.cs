namespace Uccs.Uos
{
	internal class NodeCommand : UosCommand
	{
		public const string Keyword = "node";

		public NodeCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
		{
			var run = new CommandAction {Names = ["r", "run"]};

			run.Execute = () =>	{
									if(ConsoleAvailable)
									{
										Uos.LogView.StartListening(Flow.Log);
	
										while(Flow.Active)
										{
											Console.Write("uos >");
	
											try
											{
												var x = new Xon(Console.ReadLine());
	
												if(x.Nodes[0].Name == Keyword && (
																					run.Names.Contains(x.Nodes[1].Name) 
																				 ))
													throw new Exception("Not available");
	
												Uos.Execute(x.Nodes, flow);
											}
											catch(Exception ex)
											{
												Flow.Log.ReportError(this, "Error", ex);
											}
										}

										Uos.LogView.StopListening();
									}
									else
										WaitHandle.WaitAny([Flow.Cancellation.WaitHandle]);

									return null;
								};

			run.Help = new Help(){	Title = "RUN",
									Description = "Runs a new instance with command-line interface",
									Syntax = $"{Keyword} {run.NamesSyntax} flags [profile=PATH] [net=ZONE]",

									Arguments =
									[
										new ("profile", "Path to local profile directory"),
										new ("net", "Network net to connect")
									],

									Examples =
									[
										new (null, $"{Keyword} {run.Names[1]} profile=C:\\User\\sun interzone=Testzone")
									]};
			
			Actions = [run];
		
		}
	}
}