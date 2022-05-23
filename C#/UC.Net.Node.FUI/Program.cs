using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using RocksDbSharp;

namespace UC.Net.Node.FUI
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

			try
			{
				var exedir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

				var b = new XonDocument(new XonTextReader(File.ReadAllText(Path.Combine(exedir, "Boot.xon"))));
				var cmd = new XonDocument(new XonTextReader(string.Join(' ', Environment.GetCommandLineArgs().Skip(1))));
				var boot = new BootArguments(b, cmd);

				var orig = Path.Combine(exedir, Core.SettingsFileName);
				var user = Path.Combine(boot.Main.Profile, Core.SettingsFileName);

				if(!File.Exists(user))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(user));
					File.Copy(orig, user);
				}

				var settings = new Settings(boot);

				Cryptography.Current = settings.Cryptography;

				var log = new Log();
				var core = new Core(settings, exedir, log, new RealTimeClock(), new Nas(settings, log), new EthereumFeeForm(), new FeeForm(settings));

				//if(isnode)
				//{
					core.RunServer();
					core.RunNode();
				//} 
				//else
				//{
				//	core.RunClient(overridedispatcher);
				//}

				var f = new MainForm(core);
				f.StartPosition = FormStartPosition.CenterScreen;

				f.Closed  +=	(s, a) =>
								{
									(s as MainForm).Core.Stop("Form closed");
								};

				Application.Run(f);
			}
			catch(RequirementException ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
