using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Uccs.Rdn.CLI;

public class DevCommand : RdnCommand
{
	public DevCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Ping()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "ping";
		a.Execute = () =>	{
								string host = Args[0].Name;
								var s = host.Split(':');
	
								for(int i=0; i<4; i++)
								{
									try
									{
										var client = new TcpClient(new IPEndPoint(IPAddress.Any, 0));
										client.ReceiveTimeout = 5000;
										client.SendTimeout = 5000;

										var t = DateTime.Now;

										client.Connect(IPAddress.Parse(s[0]), s.Length > 1 ? int.Parse(s[1]) : Program.Net.PpiPort);
			
										Report($"Succeeded in {(DateTime.Now - t).TotalMilliseconds:0.} ms");

										client.Close();
									}
									catch(Exception ex)
									{
										Console.WriteLine(ex.Message);
									}
								}
								return null;
							};
		return a;
	}

	public CommandAction Listen()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "listen";
		a.Execute = () =>	{
								var host = Args[0].Name;
								var s = host.Split(':');

								var Listener = new TcpListener(IPAddress.Parse(s[0]), s.Length > 1 ? int.Parse(s[1]) : Program.Net.PpiPort);
								Listener.Start();

								Report($"Listening...");

								Listener.AcceptTcpClient();

								return null;
							};
		return a;
	}

	public CommandAction Keypair()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "keypair";
		a.Execute = () =>	{
								var k = AccountKey.Create();

								Report("Public Address - " + k.ToString()); 
								Report("Private Key    - " + k.PrivateKey.ToHex());
								return null;
							};
		return a;
	}

}
