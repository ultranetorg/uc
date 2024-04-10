using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class DevCommand : Command
	{
		public const string Keyword = "dev";

		public DevCommand(Program program, List<Xon> args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.First().Name)
			{
				case "ping": 
				{
					string host = GetString(Args[1].Name);
					var s = host.Split(':');
		
					for(int i=0; i<4; i++)
					{
						try
						{
							var client = new TcpClient(new IPEndPoint(IPAddress.Any, 0));
							client.ReceiveTimeout = 5000;
							client.SendTimeout = 5000;
	
							var t = DateTime.Now;
	
							client.Connect(IPAddress.Parse(s[0]), s.Length > 1 ? int.Parse(s[1]) : Program.Zone.Port);
				
							Workflow.Log?.Report(this, null, $"Succeeded in {(DateTime.Now - t).TotalMilliseconds:0.} ms");
	
							client.Close();
						}
						catch(Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					}
					return null;
				}
				case "listen" :
				{
					var host = Args[1].Name;
					var s = host.Split(':');

					var Listener = new TcpListener(IPAddress.Parse(s[0]), s.Length > 1 ? int.Parse(s[1]) : Program.Zone.Port);
					Listener.Start();

					Workflow.Log?.Report(this, null, $"Listening...");

					Listener.AcceptTcpClient();

					return null;
				}
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
