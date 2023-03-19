using UC;
using UC.Net;

namespace Uccs.Demo.Application
{
	public static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			new Nexus(null, Zone.Localnet);

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			System.Windows.Forms.Application.Run(new Form1());
		}

		public static void SimulateMain(string productspath)
		{
			new Nexus(productspath, Zone.Localnet);
			var f = new Form1();
			f.Show();
		}
	}
}