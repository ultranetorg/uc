using System.Reflection;

namespace Uccs.Uos;

internal class ProcessCommand : UosCommand
{
	public ProcessCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
	{
	}

	public CommandAction Run()
	{
 		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "r";
		a.Help = new() {Description = "Runs a new instance with command-line interface",
						Syntax = $"{Keyword} {a.NamesSyntax} flags [profile={DIRPATH}]",

						Arguments =	[
										new ("profile", "Path to local profile directory"),
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} profile=C:\\NodeProfile")
									]};

		a.Execute = () =>	{
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
																				a.Names.Contains(x.Nodes[1].Name) 
																			 ))
												throw new Exception("Not available");

											Uos.Execute(x.Nodes, Flow);
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
		
		return a;
	}
}