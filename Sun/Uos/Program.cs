using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.Application
{
	class Program
	{
		static Settings		Settings = null;
		static Log			Log = new();
		static Core			Core;
		//ManualResetEvent	CancelUosServer = new ManualResetEvent(false);

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
					
				var b = new XonDocument(File.ReadAllText(Path.Combine(exedir, "Boot.xon")));
				var cmd = new XonDocument(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));
				var boot = new BootArguments(b, cmd);

				Settings = new Settings(exedir, boot);

				Log.Stream = new FileStream(Path.Combine(boot.Profile, "Log.txt"), FileMode.Create);
									
				if(File.Exists(Settings.Profile))
					foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Core.FailureExt))
						File.Delete(i);

				Core =	new Core(Settings, Log)
						{
							Clock = new RealTimeClock(), 
							Nas = new Nas(Settings, Log), 
						}; 

				Core.RunApi();
				Core.RunNode();

				RunUosServer();
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
				Core.Stop("The End");
			}
		}

// 		public static void WaitForConnectionEx(this NamedPipeServerStream stream, ManualResetEvent cancelEvent)
// 		{
// 			Exception e = null;
// 			AutoResetEvent connectEvent = new AutoResetEvent(false);
// 			stream.BeginWaitForConnection(ar =>
// 			{
// 				try
// 				{
// 					stream.EndWaitForConnection(ar);
// 				}
// 				catch (Exception er)
// 				{
// 					e = er;
// 				}
// 				connectEvent.Set();
// 			}, null);
// 			if (WaitHandle.WaitAny(new WaitHandle[] { connectEvent, cancelEvent }) == 1)
// 				stream.Close();
// 			if (e != null)
// 				throw e; // rethrow exception
// 		}

		static void RunUosServer()
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
