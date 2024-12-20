using System.Net;
using System.Net.Sockets;

namespace Uccs.Rdn.CLI;

public class DevCommand : RdnCommand
{
	public const string Keyword = "dev";

	public DevCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["ping"],

							Execute = () =>	{
												string host = GetString(Args[0].Name);
												var s = host.Split(':');
	
												for(int i=0; i<4; i++)
												{
													try
													{
														var client = new TcpClient(new IPEndPoint(IPAddress.Any, 0));
														client.ReceiveTimeout = 5000;
														client.SendTimeout = 5000;

														var t = DateTime.Now;

														client.Connect(IPAddress.Parse(s[0]), s.Length > 1 ? int.Parse(s[1]) : Program.Net.Port);
			
														Report($"Succeeded in {(DateTime.Now - t).TotalMilliseconds:0.} ms");

														client.Close();
													}
													catch(Exception ex)
													{
														Console.WriteLine(ex.Message);
													}
												}
												return null;
											}
						},

						new ()
						{
							Names = ["listen"],

							Execute = () =>	{
												var host = Args[0].Name;
												var s = host.Split(':');

												var Listener = new TcpListener(IPAddress.Parse(s[0]), s.Length > 1 ? int.Parse(s[1]) : Program.Net.Port);
												Listener.Start();

												Report($"Listening...");

												Listener.AcceptTcpClient();

												return null;
											}
						},

						new ()
						{
							Names = ["keypair"],

							Help = new Help
							{
							},

							Execute = () =>	{
												var k = AccountKey.Create();

												Report("Public Address - " + k.ToString()); 
												Report("Private Key    - " + k.GetPrivateKeyAsBytes().ToHex());
												return null;
											}
						},

					];
	}
}
