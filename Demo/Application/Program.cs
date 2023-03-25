using System.Reflection;
using UC;
using UC.Net;
using UC.Uos;

namespace Uccs.Demo.Application
{
	public static class Program
	{
		static UC.Uos.Application? Application;

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			Application = new UC.Uos.Application();

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			System.Windows.Forms.Application.Run(new Form1());
		}

		public static void SimulateMain()
		{
			Application = new UC.Uos.Application();
			
			var f = new Form1();


			Task.Run(() =>
					 {
						var v = ReleaseAddress.Parse("uo/democomponent/dotnet/0.0.0");
						
						Application.GetRelease(v, new Workflow());

						f.BeginInvoke(new Action(	() =>
													{
														var a = Assembly.LoadFile(Path.Join(Application.MapReleasePath(v), "Uccs.Demo.Component.dll"));
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