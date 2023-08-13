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

namespace Uccs.Sun.FUI
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

				var boot = new Boot(exedir);

				var settings = new Settings(exedir, boot);

				var log = new Log();
				var sun =	new Net.Sun(boot.Zone, settings, log)
							{
								Clock = new RealTimeClock(), 
								Nas = new Nas(settings, log), 
								GasAsker = new EthereumFeeForm(), 
								FeeAsker = new FeeForm(boot.Zone)
							}; 

				sun.RunApi();
				sun.RunNode();

				var f = new MainForm(sun);
				f.StartPosition = FormStartPosition.CenterScreen;

				f.Closed  +=	(s, a) =>
								{
									(s as MainForm).Sun.Stop("Form closed");
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
