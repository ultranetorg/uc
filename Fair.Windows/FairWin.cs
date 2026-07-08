using Uccs.Mcv.FUI;

namespace Uccs.Fair.Windows;

public class FairWin : FairCli
{
	[STAThread]
	static void Main(string[] args)
	{
		new FairWin();
	}

	public FairWin() : base()
	{
	}

	public override void InteractOrWait(string profile, Command command, Command.CommandAction action, Flow flow)
	{
		Node.ShowGui = () => {
								var t = Node.CreateThread(() => {
																	ApplicationConfiguration.Initialize();
																 
																 	System.Windows.Forms.Application.Run(new McvForm(Node));
																});
								t.Name = $"{Node.Name} FUI";
								t.SetApartmentState(ApartmentState.STA);
								t.Start();
							 };

		base.InteractOrWait(Settings.Profile, command, action, flow);
	}
}
