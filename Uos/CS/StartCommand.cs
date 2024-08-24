namespace Uccs.Uos
{
	public class StartCommand : UosCommand
	{
		public const string Keyword = "start";

		public StartCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
		{
			var run = new CommandAction {Names = ["s", "start"]};

			run.Execute = () =>	{
									uos.Start(Ura.Parse(Args[0].Name), Flow);

									return null;
								};

			run.Help = new Help(){	Title = "Start",
									Description = "",
									Syntax = $"{Keyword} {run.NamesSyntax}",

									Arguments =
									[
										new ("<first>", "Path to resource to execute"),
									],

									Examples =
									[
										new (null, $"{Keyword} {run.Names[1]} C:\\User\\sun interzone=Testzone")
									]};
			
			Actions = [run];
		
		}
	}
}