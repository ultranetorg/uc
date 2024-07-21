using System.Diagnostics;

namespace Uccs.Uos
{
	public class AprvCommand : UosCommand
	{
		public const string Keyword = "aprv";

		public AprvCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
		{
			var install = new CommandAction(){
												Names = ["i", "install"],

												Help = new Help
												{ 
													Title = "INSTALL",
													Description = "If needed, downloads specified package and its dependencies recursively and unpacks its content to the 'Packages' directory",
													Syntax = "release i|install PACKAGE_ADDRESS",

													Arguments =
													[
														new ("<first>", "Address of a package to install")
													],

													Examples =
													[
														new (null, "package i company/application/windows/1.2.3")
													]
												},

												Execute = () =>	{
																	uos.Install(AprvAddress.Parse(Args[0].Name), Flow);

																	return null;
																}
											};


			Actions = [install];
		
		}
	}
}