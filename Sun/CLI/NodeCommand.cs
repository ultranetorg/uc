﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class NodeCommand : Command
	{
		public const string Keyword = "node";

		public NodeCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			switch(Args.Nodes[0].Name)
			{
				case "run" :
				{
					var b = new Boot(Program.ExeDirectory);
					var s = new Settings(Program.ExeDirectory, b);
	
					Program.Sun = new Net.Sun(b.Zone, s, Workflow){	Clock = new RealClock(),
																	Nas = new Nas(s),
																	GasAsker = ConsoleAvailable ? new ConsoleGasAsker() : new SilentGasAsker(),
																	FeeAsker = new SilentFeeAsker() };
					Program.Sun.Run(Args);
	
					if(ConsoleAvailable)
						while(Workflow.Active)
						{
							Console.Write(b.Zone + " > ");
	
							var l = new Log();
							var v = new ConsoleLogView(false, true);
							v.StartListening(l);
	
							try
							{
								var x = new XonDocument(Console.ReadLine());
	
								if(x.Nodes[0].Name == Keyword)
									throw new Exception("Not available");
	
								Program.Execute(x, l);
							}
							catch(Exception ex)
							{
								l.ReportError(this, "Error", ex);
							}
	
							v.StopListening(l);
						}
					else
						WaitHandle.WaitAny(new WaitHandle[] {Workflow.Cancellation.WaitHandle});
				
					break;;
				}

				case "attach" :
				{
					var a = new Uri(Args.Nodes[1].Name);

					var h = new HttpClientHandler();
					h.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
					var http = new HttpClient(h){Timeout = TimeSpan.FromSeconds(60)};

					Program.ApiClient = new JsonApiClient(http, Args.Nodes[1].Name, GetString("accesskey", null));

					var v = new ConsoleLogView(false, true);
					v.StartListening(Workflow.Log);

					while(true)
					{
						Console.Write($"{a.Host}:{a.Port} > ");
						var c = Console.ReadLine();

						if(c == "exit")
							break;

						try
						{
							var x = new XonDocument(c);

							if(x.Nodes[0].Name == Keyword || x.Nodes[0].Name == LogCommand.Keyword)
								throw new Exception("Not available");
	
							Program.Execute(x);
						}
						catch(Exception ex)
						{
							Workflow.Log.ReportError(this, "Error", ex);
						}
					}

					v.StopListening(Workflow.Log);

					break;
				}
			}

			return null;
		}
	}
}