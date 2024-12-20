﻿using System;
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
		static Net.Sun		Sun;
		//ManualResetEvent	CancelUosServer = new ManualResetEvent(false);

		private static int numThreads = 1;

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			var exedir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			var b = new Boot(exedir);
			
			try
			{

				Settings = new Settings(exedir, b);

				Log.Reported += m => File.AppendAllText(Path.Combine(Settings.Profile, "Sun.log"), m.ToString() + Environment.NewLine);

				Sun = new Net.Sun(b.Zone, Settings, new Workflow("Main", Log)){	Clock = new RealClock(), 
																					Nas = new Nas(Settings), }; 

				Sun.RunApi();
				Sun.RunNode(Role.Chain);

				RunUosServer();
			}
			catch(OperationCanceledException)
			{
				Log.ReportError(null, "Execution aborted");
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				var m = Path.GetInvalidFileNameChars().Aggregate(MethodBase.GetCurrentMethod().Name, (c1, c2) => c1.Replace(c2, '_'));
				File.WriteAllText(Path.Join(b.Profile, m + "." + Net.Sun.FailureExt), ex.ToString());
			}
	
			if(Sun != null)
			{
				Sun.Stop("The End");
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

			while(Sun.Workflow.Active)
			{
				try
				{
					var m = Message.FromType((MessageType)r.ReadByte());
		
					m.Read(r);
		
					switch(m as object)
					{
						case StopMessage:
						{
							Sun.Stop("By UOS request");
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
