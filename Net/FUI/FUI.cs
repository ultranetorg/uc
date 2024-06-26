using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Uccs.Net.FUI
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

// 			try
// 			{
// 				var exedir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
// 				var b = new Boot(exedir);
// 				var s = new SunSettings(exedir, b);
// 
// 				var sun = new Net.Sun(s, new Flow("Main", new Log())); 
// 				
// 				sun.Run(b.Commnand.Nodes);
// 
// 				var f = new MainForm(sun);
// 				f.StartPosition = FormStartPosition.CenterScreen;
// 
// 				f.Closed += (s, a) => (s as MainForm).Sun.Stop("Form closed");
// 
// 				Application.Run(f);
// 			}
// 			catch(RequirementException ex)
// 			{
// 				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
// 			}
		}
	}
}
