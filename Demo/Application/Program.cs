using System.Diagnostics;
using System.Reflection;
using Uccs.Net;
using Uccs.Nexus;
using Uccs.Rdn;

//using Uccs.Rdn;

namespace Uccs.Demo.Application
{
	public static class Program
	{
		static Uccs.Nexus.Application Application;

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			ApplicationConfiguration.Initialize();

			Application = new ();
 			
 			var f = new Form1();
 			
            var flow = new Flow("SimulateMain");

			
 			Task.Run(() =>	{
                                var d = Application.Nexus.Nexus.FindLocalPackage(Application.Address, flow).Manifest.CompleteDependencies.FirstOrDefault(i => new ApvAddress(i.Address).Product == "demo.component/dotnet");
                                var p = Application.Nexus.Nexus.FindLocalPackage(d.Address, flow);
 			
								///if(p == null || !p.Available)
								{
									p = Application.Nexus.Nexus.DeployPackage(d.Address, Application.Nexus.Settings.Packages, flow);
								}

 								f.BeginInvoke(() =>	{
 														var a = Assembly.LoadFile(Path.Join(Application.Nexus.AddressToDeployment(Application.Nexus.Settings.Packages, new (d.Address)), "Uccs.Demo.Component.dll"));
 														var cc = a.GetType("Uccs.Demo.Component.ComponentControl");
 														var c = cc.GetConstructor([]).Invoke(null) as UserControl;
 														
 														c.Location = f.pictureBox1.Location;
 														c.Size = f.pictureBox1.Size;
 														
 														f.Controls.Remove(f.pictureBox1);
 														f.Controls.Add(c);
 													});
 							});
 			
 			//f.Show();

			System.Windows.Forms.Application.Run(f);
		}
	}
}