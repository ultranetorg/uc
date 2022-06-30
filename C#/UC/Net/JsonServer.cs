using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Encoders;

namespace UC.Net
{
	public class JsonServer
	{
		Core			Core;
		HttpListener	Listener;
		Thread			Thread;

		Log				Log  => Core.Log;
		Settings		Settings => Core.Settings;
		Vault			Vault => Core.Vault;
		Roundchain		Chain => Core.Chain;

		public JsonServer(Core core)
		{
			Core = core;

			Thread = new Thread(() =>
								{ 
									try
									{
										Listener = new HttpListener();
	
										var prefixes = new string[] {$"http://{(Settings.IP.ToString() != "0.0.0.0" ? Settings.IP.ToString() : "+")}:{Zone.JsonPort(Settings.Zone)}/"};
			
										foreach(string s in prefixes)
										{
											Listener.Prefixes.Add(s);
										}
	
										Listener.Start();
				
										Log?.Report(this, "Listening started", prefixes[0]);
		
										while(Core.Working)
										{
											ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessRequest), Listener.GetContext()); 
										}
									}
									catch(SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
									{
										Listener = null;
									}
									catch(HttpListenerException ex) when (ex.NativeErrorCode == 995 || ex.NativeErrorCode == 500)
									{
										Listener = null;
									}
									catch(Exception ex) when (!Debugger.IsAttached)
									{
										if(!Listener.IsListening)
											Listener = null;

										Core.Stop(MethodBase.GetCurrentMethod(), ex);
									}
								});

			Thread.Name = $"{Settings.IP.GetAddressBytes()[3]} Aping";
			Thread.Start();
		}

		public void Stop()
		{
			Listener?.Stop();
		}

		public void WaitStop()
		{
			Thread?.Join();
		}

		void ProcessRequest(object obj)
		{
			var context = obj as HttpListenerContext;

			var rq = context.Request;
			var rp = context.Response;
	
			void responderror(string t, int code = 599)	{
															try
															{
																rp.StatusCode = code;

																var output = rp.OutputStream;
																var buffer = Encoding.UTF8.GetBytes(t);
							
																rp.ContentType = "text/plain" ;
																rp.ContentLength64 = buffer.Length;

																output.Write(buffer, 0, buffer.Length);
															}
															catch(InvalidOperationException)
															{
															}
														}
	
			void respondjson(dynamic t){
											try
											{
												var output = rp.OutputStream;
												var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(t, JsonClient.Options));
							
												rp.ContentType = "text/json" ;
												rp.ContentLength64 = buffer.Length;

												output.Write(buffer, 0, buffer.Length);
											}
											catch(InvalidOperationException)
											{
											}
									}

			void respondbinary(byte[] t){
											try
											{
												rp.ContentType = "application/octet-stream";
						
												rp.OutputStream.Write(t, 0, t.Length);
											}
											catch(InvalidOperationException)
											{
											}
										}
	
			if(rq.ContentType == null || !rq.HasEntityBody)
				return;
	
			var reader = new StreamReader(rq.InputStream, rq.ContentEncoding);
		
			try
			{
				var json = reader.ReadToEnd();
				
				var call = JsonSerializer.Deserialize(json, Type.GetType(GetType().Namespace + "." + rq.Url.LocalPath.Substring(1) + "Call"), JsonClient.Options) as RpcCall;

				if(call.Private && (string.IsNullOrWhiteSpace(Settings.Api.AccessKey) || call.AccessKey != Settings.Api.AccessKey))
				{
					rp.StatusCode = (int)HttpStatusCode.Unauthorized;
					rp.Close();
					return;
				}

				rp.StatusCode = (int)HttpStatusCode.OK;

				lock(Core.Lock)
					switch(call)
					{
						case RunNodeCall e:
							Core.RunNode();
							break;

						case AddWalletCall e:
							Vault.AddWallet(e.Account, e.Wallet);
							break;

						case UnlockWalletCall e:
							Vault.Unlock(e.Account, e.Password);
							break;
	
						case SetGeneratorCall e:
							Core.Settings.Generator = Vault.GetPrivate(e.Account).Key.GetPrivateKey();
							Log?.Report(this, "Generator is set", e.Account.ToString());
							break;

						case TransferUntCall e:
							respondjson(Core.Enqueue(new UntTransfer(Vault.GetPrivate(e.From), e.To, e.Amount)));
							Log?.Report(this, "TransferUnt received", $"{e.From} -> {e.Amount} -> {e.To}");
							break;
	
						case StatusCall s:
							respondjson(new GetStatusResponse {	Log			= Log?.Messages.TakeLast(s.Limit).Select(i => i.ToString()), 
																Rounds		= Chain.Rounds.Take(s.Limit).Reverse().Select(i => i.ToString()), 
																InfoFields	= Core.Info[0].Take(s.Limit), 
																InfoValues	= Core.Info[1].Take(s.Limit) });
							break;
							
// 						case AccountInfoCall c:
// 							if(Core.Synchronization != Synchronization.Synchronized)
// 								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
// 							else
// 							{
// 								var ai = Chain.GetAccountInfo(c.Account, c.Confirmed);
// 								
// 								if(ai != null)
// 									respondjson(ai);
// 								else
// 									responderror("Account not found");
// 							}
// 							break;
// 
// 						case AuthorInfoCall c:
// 							if(Core.Synchronization != Synchronization.Synchronized)
// 								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
// 							else
// 							{
// 								var ai = Chain.GetAuthorInfo(c.Name, c.Confirmed);
// 								
// 								if(ai != null)
// 									respondjson(ai);
// 								else
// 									responderror("Author not found");
// 							}
// 							break;

// 						case DelegatePropositionCall c:
// 							if(Core.Synchronization != Synchronization.Synchronized)
// 								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
// 							else
// 							{
// 								Core.ProcessIncoming(c.Propositions);
// 							}
// 							break;

//						case QueryReleaseCall c:
//							if(Core.Synchronization != Synchronization.Synchronized)
//								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
//							else
//							{
//								var ai = c.Queries.Select(i => Chain.QueryRelease(i, c.Confirmed));
//								
//								respondjson(ai);
//							}
//							break;

// 						case DownloadPackageRequest c:
// 						{
// 							///var ai = Core.Api.Send(new DownloadPackageRequest(c.Request);							
// 							///respondbinary(ai);
// 							break;
// 						}
						case ExitCall e:
							rp.Close();
							Core.Stop("RPC call");
							return;
	
						default:
							rp.StatusCode = (int)HttpStatusCode.NotFound;
							break;
					}
			}
			catch(HttpListenerException)
			{
			}
			catch(ObjectDisposedException)
			{ 
			}
			catch(JsonException ex)
			{
				responderror(ex.Message, (int)HttpStatusCode.BadRequest);
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				responderror(ex.ToString(), (int)HttpStatusCode.InternalServerError);
			}

			try
			{
				rp.Close();
			}
			catch(ObjectDisposedException){}
			catch(InvalidOperationException){}
		}
	}
}
