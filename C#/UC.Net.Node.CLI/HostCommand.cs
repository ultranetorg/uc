using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace UC.Net.Node.CLI
{
	internal class HostCommand : Command
	{
		public const string Keyword = "host";

		/// <summary>
		/// Usage:	host ping ip=IP[:PORT]
		/// </summary>
		public HostCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "ping": 
					string host = GetString("ip");

					var s = host.Split(':');
		
					for(int i=0; i<4; i++)
					{
						try
						{
							var client = new TcpClient(new IPEndPoint(IPAddress.Any, 0));
							client.ReceiveTimeout = 5000;
							client.SendTimeout = 5000;
	
							var t = DateTime.Now;
	
							client.Connect(IPAddress.Parse(s[0]), s.Length > 1 ? int.Parse(s[1]) : Zone.Port(Settings.Zone));
				
							Log.Report(this, null, $"Succeeded in {(DateTime.Now - t).TotalMilliseconds:0.} ms");
	
							client.Close();
						}
						catch(Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					}
					return null;

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
