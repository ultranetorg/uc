using Nethereum.Signer;
using Nethereum.Web3;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace UC.Net.Node.CLI
{
	class Program
	{
		static Settings		Settings = null;
		static Log			Log = new();
		static Core			Core;

		private static int numThreads = 1;

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			var exedir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			try
			{
				foreach(var i in Directory.EnumerateFiles(exedir, "*." + Core.FailureExt))
					File.Delete(i);
					
				var b = new XonDocument(new XonTextReader(File.ReadAllText(Path.Combine(exedir, "Boot.xon"))), XonTextValueSerializator.Default);
				var cmd = new XonDocument(new XonTextReader(string.Join(' ', Environment.GetCommandLineArgs().Skip(1))), XonTextValueSerializator.Default);
				var boot = new BootArguments(b, cmd);

				var orig = Path.Combine(exedir, Settings.FileName);

				Log.Stream = new FileStream(Path.Combine(boot.Profile, "Log.txt"), FileMode.Create);

				Settings = new Settings(boot);

				Cryptography.Current = Settings.Cryptography;
									
				if(File.Exists(Settings.Profile))
					foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Core.FailureExt))
						File.Delete(i);

				string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

				Core =	new Core(Settings, dir, Log)
						{
							Clock = new RealTimeClock(), 
							Nas = new Nas(Settings, Log), 
						}; 

				Core.RunApi();
				Core.RunNode();

// 				Task.Run(() =>	{
// 									while(Core.Running)
// 									{
// 										Thread.Sleep(100); 
// 									}
// 								})
// 								.Wait();

				StartUosServer();

				Core.Stop("The End");
			}
			catch(AbortException)
			{
				Log.ReportError(null, "Execution aborted");
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				var m = Path.GetInvalidFileNameChars().Aggregate(MethodBase.GetCurrentMethod().Name, (c1, c2) => c1.Replace(c2, '_'));
				File.WriteAllText(Path.Join(Settings?.Profile ?? exedir, m + "." + Core.FailureExt), ex.ToString());
				throw;
			}
	
			if(Core != null)
			{
				Core.Stop("End");
			}
		}

		static void StartUosServer()
		{
			var pipe = new NamedPipeServerStream("UOS-A-" + Process.GetCurrentProcess().Id, PipeDirection.InOut, numThreads, PipeTransmissionMode.Message);

			pipe.WaitForConnection();

			var r = new BinaryReader(pipe);

			while(Core.Running)
			{
				try
				{
					var m = Message.FromType((MessageType)r.ReadByte());
		
					m.Read(r);
		
					switch(m as object)
					{
						case StopMessage:
						{
							Core.Stop("By UOS request");
							goto stop;
						}
					}
				}
				catch(EndOfStreamException)
				{
					break;
				}
			}

			stop:
				pipe.Close();
		}
	}
}
