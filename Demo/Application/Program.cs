using System.Reflection;
using Uccs.Rdn;

//using Uccs.Rdn;

namespace Uccs.Demo.Application
{
	public static class Program
	{
		static Uccs.Uos.Application Application;

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			ApplicationConfiguration.Initialize();

			Application = new Uccs.Uos.Application();

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.

 			Application = new Uccs.Uos.Application();
 			
 			var f = new Form1();
 			
            var flow = new Flow("SimulateMain");

 			Task.Run(() =>	{
                                var d = Application.Nexus.Rdn.FindLocalPackage(Application.Address, flow).Manifest.CompleteDependencies.FirstOrDefault(i => new AprvAddress(i.Address).Product == "demo.component");
                                var p = Application.Nexus.Rdn.FindLocalPackage(new AprvAddress(d.Address), flow);
 			
								///if(p == null || !p.Available)
								{
									p = Application.Nexus.Rdn.DeployPackage(new AprvAddress(d.Address), Application.PackagesPath, flow);
								}

 								f.BeginInvoke(() =>	{
 														var a = Assembly.LoadFile(Path.Join(PackageHub.AddressToDeployment(Application.PackagesPath, new (d.Address)), "Uccs.Demo.Component.dll"));
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