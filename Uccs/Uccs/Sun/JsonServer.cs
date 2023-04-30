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
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Encoders;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Uccs.Net.ChainReportResponse;

namespace Uccs.Net
{
	public class JsonServer
	{
		Core			Core;
		HttpListener	Listener;
		Thread			Thread;

		Workflow		Workflow => Core.Workflow;
		Settings		Settings => Core.Settings;
		Vault			Vault => Core.Vault;
		Database		Database => Core.Database;

		public JsonServer(Core core)
		{
			Core = core;

			Thread = new Thread(() =>
								{ 
									try
									{
										Listener = new HttpListener();
	
										var prefixes = new string[] {$"http://{Settings.IP}:{Core.Zone.JsonPort}/"};
			
										foreach(string s in prefixes)
										{
											Listener.Prefixes.Add(s);
										}

										Listener.Start();
				
										Workflow.Log?.Report(this, "Listening started", prefixes[0]);
		
										while(!Workflow.IsAborted)
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

																var buffer = Encoding.UTF8.GetBytes(t);
							
																rp.StatusCode = code;
																rp.ContentType = "text/plain" ;
																rp.ContentLength64 = buffer.Length;

																rp.OutputStream.Write(buffer, 0, buffer.Length);
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

				if(!string.IsNullOrWhiteSpace(Settings.Api.AccessKey) && call.AccessKey != Settings.Api.AccessKey)
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
							lock(Core.Lock)
								return new SettingsResponse {ProfilePath	= Core.Settings.Profile, 
															 Settings		= Core.Settings}; /// TODO: serialize
	
						case RunNodeCall e:
							Core.RunNode();
							break;

						case ExitCall e:
							rp.Close();
							Core.Stop("Json Api call");
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
								Core.Settings.Generators = e.Generators.Select(i => Vault.GetKey(i)).ToList();
							
							Workflow.Log?.Report(this, "Generators is set", string.Join(", ", e.Generators));
							break;
	
						case UntTransferCall e:
							
							AccountKey  pa;
								
							lock(Core.Lock)
							{
								pa = Vault.GetKey(e.From);
							}

							Workflow.Log?.Report(this, "TransferUnt received", $"{e.From} -> {e.Amount} -> {e.To}");
							return Core.Enqueue(new UntTransfer(pa, e.To, e.Amount), PlacingStage.Accepted, new Workflow());
		
						case LogReportCall s:
							return new LogResponse{Log = Workflow.Log?.Messages.TakeLast(s.Limit).Select(i => i.ToString()).ToArray() }; 

						case SummaryReportCall s:
							lock(Core.Lock)
								return new SummaryResponse{Summary = Core.Summary.Take(s.Limit).Select(i => new [] {i.Key, i.Value}).ToArray() }; 

						case PeersReportCall s:
							return new PeersResponse{Peers = Core.Peers.TakeLast(s.Limit).Select(i => i.ToString()).ToArray()}; 
							
						case ChainReportCall s:
							lock(Core.Lock)
								return new ChainReportResponse{Rounds = Database?.Tail	.Take(s.Limit)
																						.Reverse()
																						.Select(i => new ChainReportResponse.Round
																									{
																										Id = i.Id, 
																										Members = i.Members.Count,
																										Pieces = i.BlockPieces.Count,
																										Voted = i.Voted,
																										Confirmed = i.Confirmed,
																										Time = i.Time,
																										Blocks = i.Blocks.Select(i => new ChainReportResponse.Block {Generator = i.Generator.ToString(), Type = i.Type}),
																									})
																						.ToArray()}; 
							
						case PiecesReportCall s:
							lock(Core.Lock)
								return new PiecesReportResponse{Pieces = Database?	.FindRound(s.RoundId)?.BlockPieces
																					.OrderBy(i => i.Generator)
																					.Take(s.Limit)
																					.Select(i => new PiecesReportResponse.Piece
																					{
																						Type = i.Type,
																						Try = i.Try,
																						RoundId = i.RoundId,
																						Index = i.Index,
																						Total = i.Total,
																						Signature = i.Signature,
																						DataLength = i.Data.Length,
																						Generator = i.Generator
																					})
																					.ToArray()}; 
	
						case QueryReleaseCall c:
							lock(Core.Lock)
								return Core.QueryRelease(c.Queries, c.Confirmed);
	
						case AddReleaseCall c:
							lock(Core.Lock)
							{
								Core.Filebase.AddRelease(c.Release, c.Manifest);
			
								if(c.Complete != null)
								{
									Core.Filebase.WritePackage(c.Release, Distributive.Complete, 0, c.Complete);
								}
								if(c.Incremental != null)
								{
									Core.Filebase.WritePackage(c.Release, Distributive.Incremental, 0, c.Incremental);
								}
							}
	
							break;
	
						case DownloadReleaseCall c:
							lock(Core.Lock)
								Core.DownloadRelease(c.Release, Workflow);
							break;
	
						case ReleaseStatusCall c:
							lock(Core.Lock)
								return Core.GetReleaseStatus(c.Release);

						case GetReleaseCall c:
							lock(Core.Lock)
								Core.GetRelease(c.Version, Workflow);
							break;
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

					lock(Core.Lock)
						respondjson(rs);
				}
				else
				{
					var r = execute(call);
	
					if(r != null)
					{
						lock(Core.Lock)
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
