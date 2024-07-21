using System.Diagnostics;

namespace Uccs.Uos
{
	internal class StartCommand : UosCommand
	{
		public const string Keyword = "start";

		public StartCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
		{
			var run = new CommandAction {Names = ["s", "start"]};

			run.Execute = () =>	{
// 									var u = Ura.Parse(Args[2].Name);
// 									//var p = PackageAddress.Parse(Args[2].Name);
// 
// 									var h = new HttpClientHandler();
// 									h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
// 									var c = new HttpClient(h) {Timeout = TimeSpan.FromSeconds(5*60)};
// 
// 									var a = new RdnApiClient(c, uos.Find<RdnNode>().Settings.Api.ListenAddress, uos.Find<RdnNode>().Settings.Api.AccessKey);
// 
// 									var pi = a.Request<PackageInfo>(new PackageInfoApc {Package = u}, Flow);
// 									var di = a.Request<DeploymentInfoApc.Result>(new DeploymentInfoApc {Package = u}, Flow);
// 									
// 									if(pi.Manifest.CompleteHash.SequenceEqual(di.Hash))
// 									{
// 										a.Send(new PackageInstallApc {Package = u}, Flow);
// 	
// 										do
// 										{
// 											var d = Api<ResourceActivityProgress>(new PackageActivityProgressApc {Package = u});
// 								
// 											if(d == null)
// 											{	
// 												if(!Api<PackageInfo>(new PackageInfoApc {Package = u}).Ready)
// 												{
// 													Flow.Log?.ReportError(this, "Failed");
// 													return null;
// 												}
// 												else
// 													break;
// 											}
// 	
// 											Report(d.ToString());
// 	
// 											Thread.Sleep(500);
// 										}
// 										while(Flow.Active);
// 									}
// 
// 									var m = Directory.EnumerateFiles(di.Path, "." + PackageManifest.Extension).First();
// 
// 									var rm = new PackageManifest();
// 									rm.Read(new BinaryReader(new MemoryStream(File.ReadAllBytes(m))));
// 
// 									var p = new Process();
// 									p.StartInfo.UseShellExecute = true;
// 									p.StartInfo.FileName = rm.Execution.Path;
// 									p.StartInfo.Arguments = rm.Execution.Arguments;
// 
// 									p.Start();

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