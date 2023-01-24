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

		Workflow		Workflow => Core.Workflow;
		Settings		Settings => Core.Settings;
		Vault			Vault => Core.Vault;
		Database		Chain => Core.Database;

		public JsonServer(Core core)
		{
			Core = core;

			Thread = new Thread(() =>
								{ 
									try
									{
										Listener = new HttpListener();
	
										var prefixes = new string[] {$"http://{(Settings.IP.ToString() != "0.0.0.0" ? Settings.IP.ToString() : "+")}:{Settings.Zone.JsonPort}/"};
			
										foreach(string s in prefixes)
										{
											Listener.Prefixes.Add(s);
										}

										Listener.Start();
				
										Workflow.Log?.Report(this, "Listening started", prefixes[0]);
		
										while(Core.Running)
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
															catch(HttpListenerException)
															{
															}
														}
	
			void respondjson(dynamic t){
											var output = rp.OutputStream;
											var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(t, JsonClient.Options));
							
											rp.ContentType = "text/json" ;
											rp.ContentLength64 = buffer.Length;

											output.Write(buffer, 0, buffer.Length);
										}

// 			void respondbinary(byte[] t){
// 											try
// 											{
// 												rp.ContentType = "application/octet-stream";
// 						
// 												rp.OutputStream.Write(t, 0, t.Length);
// 											}
// 											catch(InvalidOperationException)
// 											{
// 											}
// 										}
	
			if(rq.ContentType == null || !rq.HasEntityBody)
				return;
	
			var reader = new StreamReader(rq.InputStream, rq.ContentEncoding);
		
			try
			{
				var json = reader.ReadToEnd();
				
				var t = Type.GetType(GetType().Namespace + "." + rq.Url.LocalPath.Substring(1) + "Call");

				if(t == null)
				{
					rp.StatusCode = (int)HttpStatusCode.NotFound;
					rp.Close();
					return;
				}

				var call = JsonSerializer.Deserialize(json, t, JsonClient.Options) as ApiCall;

				if(string.IsNullOrWhiteSpace(Settings.Api.AccessKey) || call.AccessKey != Settings.Api.AccessKey)
				{
					rp.StatusCode = (int)HttpStatusCode.Unauthorized;
					rp.Close();
					return;
				}

				rp.StatusCode = (int)HttpStatusCode.OK;

				object execute(ApiCall call)
				{
					switch(call)
					{
						case SettingsCall c:
							return new SettingsResponse{ProfilePath  = Core.Settings.Profile, 
														Settings = Core.Settings}; /// TODO: serialize
	
						case RunNodeCall e:
							Core.RunNode();
							break;
	
						case AddWalletCall e:
							lock(Core.Lock)
								Vault.AddWallet(e.Account, e.Wallet);
							break;
	
						case UnlockWalletCall e:
							lock(Core.Lock)
								Vault.Unlock(e.Account, e.Password);
							break;
		
						case SetGeneratorCall e:
							lock(Core.Lock)
							{
								Core.Settings.Generators = e.Generators.Select(i => Vault.GetPrivate(i)).ToList();
								Workflow.Log?.Report(this, "Generators is set", string.Join(", ", e.Generators));
							}
							break;
	
						case TransferUntCall e:
	
							PrivateAccount  pa;
								
							lock(Core.Lock)
							{
								pa = Vault.GetPrivate(e.From);
							}
	
							Workflow.Log?.Report(this, "TransferUnt received", $"{e.From} -> {e.Amount} -> {e.To}");

							return Core.Enqueue(new UntTransfer(pa, e.To, e.Amount), PlacingStage.Accepted, null);
		
						case StatusCall s:
							lock(Core.Lock)
								return new GetStatusResponse{	Log			= Workflow.Log?.Messages.TakeLast(s.Limit).Select(i => i.ToString()), 
																Rounds		= Chain.Tail.Take(s.Limit).Reverse().Select(i => i.ToString()), 
																InfoFields	= Core.Info[0].Take(s.Limit), 
																InfoValues	= Core.Info[1].Take(s.Limit), 
																Peers		= Core.Peers.Select(i => $"{i.IP} S={i.Status} In={i.InStatus} Out={i.OutStatus} F={i.Failures}")};
								
						case ExitCall e:
							rp.Close();
							Core.Stop("Json Api call");
							break;
	
						case QueryReleaseCall c:
						{
							return Core.QueryRelease(c.Queries, c.Confirmed);
						}
	
						case DistributeReleaseCall c:
						{
							Core.Filebase.AddRelease(c.Release, c.Manifest);
	
							if(c.Complete != null)
							{
								Core.Filebase.WritePackage(c.Release, Distributive.Complete, 0, c.Complete);
								//Core.DeclareRelease(new PackageAddress[]{new PackageAddress(c.Release, Distributive.Complete)}, new Workflow());
							}
							if(c.Incremental != null)
							{
								Core.Filebase.WritePackage(c.Release, Distributive.Incremental, 0, c.Incremental);
								//Core.DeclarePackage(new PackageAddress[]{new PackageAddress(c.Release, Distributive.Incremental)}, new Workflow());
							}
	
							break;
						}
	
						case DownloadReleaseCall c:
						{
							Core.DownloadRelease(c.Release, new Workflow());
							break;
						}
	
						case ReleaseInfoCall c:
						{
							return Core.GetReleaseInfo(c.Release);
						}
					}

					return null;
				}

				if(call is BatchCall c)
				{ 
					var rs = new List<dynamic>();

					foreach(var i in c.Calls)
					{
						t = Type.GetType(GetType().Namespace + "." + i.Name + "Call");
						rs.Add(execute(JsonSerializer.Deserialize(i.Call, t, JsonClient.Options) as ApiCall));
					}

					respondjson(rs);
				}
				else
				{
					var r = execute(call);

					if(r != null)
					{
						respondjson(r);
					}
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
