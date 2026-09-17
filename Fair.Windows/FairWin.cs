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

	public override void InteractOrWait(string prompt, string profile, Command command, CommandAction action, Flow flow)
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

		base.InteractOrWait(prompt, Settings.Profile, command, action, flow);
	}
}
