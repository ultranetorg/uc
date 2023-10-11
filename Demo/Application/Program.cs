using System.Reflection;
using Uccs;
using Uccs.Net;
using Uccs.Uos;

namespace Uccs.Demo.Application
{
	public static class Program
	{
		static Uccs.Uos.Application? Application;

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			Application = new Uccs.Uos.Application();

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			System.Windows.Forms.Application.Run(new Form1());
		}

		public static void SimulateMain()
		{
			Application = new Uccs.Uos.Application();
			
			var f = new Form1();


			Task.Run(() =>
					 {
						var v = PackageAddress.Parse("uo/democomponent/dotnet/0.0.0");
						
						Application.Nexus.Sun.Send(new PackageInstallCall {Package = v}, new Workflow($"InstallPackage {v}"));

						PackageDownloadProgress s = null;

						do
						{
							Thread.Sleep(1);
							s = Application.Nexus.Sun.Request<PackageDownloadProgress>(new PackageDownloadProgressCall {Package = v}, new Workflow("GetReleaseStatus"));
						}
						while(!s.Succeeded);

						f.BeginInvoke(new Action(	() =>
													{
														var a = Assembly.LoadFile(Path.Join(Application.Nexus.PackageBase.AddressToPath(v), "Uccs.Demo.Component.dll"));
														var ct = a.GetType("DemoComponent.ComponentControl");
														var c = ct?.GetConstructor(new Type[]{})?.Invoke(null) as UserControl;

														c.Location = f.pictureBox1.Location;
														c.Size = f.pictureBox1.Size;

														f.Controls.Remove(f.pictureBox1);
														f.Controls.Add(c);
													}));
					 });

			f.Show();
		}
	}
}