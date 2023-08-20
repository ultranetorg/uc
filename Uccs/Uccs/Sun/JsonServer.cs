using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
	public class JsonServer
	{
		Sun				Sun;
		HttpListener	Listener;
		Thread			Thread;

		Workflow		Workflow => Sun.Workflow;
		Settings		Settings => Sun.Settings;
		Vault			Vault => Sun.Vault;
		Mcv				Database => Sun.Mcv;

		public JsonServer(Sun sun)
		{
			Sun = sun;

			Thread = new Thread(() =>
								{ 
									try
									{
										Listener = new HttpListener();
	
										var prefixes = new string[] {$"http://{(Settings.IP.Equals(IPAddress.Any) ? "+" : Settings.IP)}:{Sun.Zone.JsonPort}/"};
			
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

										Sun.Stop(MethodBase.GetCurrentMethod(), ex);
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
							lock(Sun.Lock)
								return new SettingsResponse {ProfilePath	= Sun.Settings.Profile, 
															 Settings		= Sun.Settings}; /// TODO: serialize
						case RunNodeCall e:
							Sun.RunNode();
							break;

						case ExitCall e:
							rp.Close();
							Sun.Stop("Json Api call");
							break;
	
						case AddWalletCall e:
							lock(Sun.Lock)
								Vault.AddWallet(e.Account, e.Wallet);
							break;
	
						case UnlockWalletCall e:
							lock(Sun.Lock)
								Vault.Unlock(e.Account, e.Password);
							break;
		
						case SetGeneratorCall e:
							lock(Sun.Lock)
								Sun.Settings.Generators = e.Generators.Select(i => Vault.GetKey(i)).ToList();
							
							Workflow.Log?.Report(this, "Generators is set", string.Join(", ", e.Generators));
							break;
	
						case UntTransferCall e:
							
							AccountKey  pa;
								
							lock(Sun.Lock)
							{
								pa = Vault.GetKey(e.From);
							}

							Workflow.Log?.Report(this, "TransferUnt received", $"{e.From} -> {e.Amount} -> {e.To}");
							return Sun.Enqueue(new UntTransfer(pa, e.To, e.Amount), PlacingStage.Accepted, new Workflow());
		
						case LogReportCall s:
							return new LogResponse{Log = Workflow.Log?.Messages.TakeLast(s.Limit).Select(i => i.ToString()).ToArray() }; 

						case SummaryReportCall s:
							lock(Sun.Lock)
								return new SummaryResponse{Summary = Sun.Summary.Take(s.Limit).Select(i => new [] {i.Key, i.Value}).ToArray() }; 

						case PeersReportCall s:
							return new PeersResponse{Peers = Sun.Peers.OrderByDescending(i => i.Established).TakeLast(s.Limit).Select(i => i.ToString()).ToArray()}; 
							
						case ChainReportCall s:
							lock(Sun.Lock)
								return new ChainReportResponse{Rounds = Database?.Tail	.Take(s.Limit)
																						.Reverse()
																						.Select(i => new ChainReportResponse.Round
																									{
																										Id = i.Id, 
																										Members = i.Members.Count,
																										Analyzers = i.Analyzers.Count,
																										Voted = i.Voted,
																										Confirmed = i.Confirmed,
																										Time = i.ConfirmedTime,
																										Hash = i.Hash,
																										Summary = i.Summary,
																										Votes = i.Votes.Select(b => new ChainReportResponse.Vote {	Generator = b.Generator, 
																																									IsPayload = b.Transactions.Any(), 
																																										/*Confirmed = i.Confirmed && i.Transactions.Any() && i.ConfirmedPayloads.Contains(b)*/ }),
																										JoinRequests = i.JoinRequests.Select(i => i.Generator),
																									})
																						.ToArray()}; 
							
						case VotesReportCall s:
							lock(Sun.Lock)
								return new VotesReportResponse{Pieces = Database?	.FindRound(s.RoundId)?.Votes
																					.OrderBy(i => i.Generator)
																					.Take(s.Limit)
																					.Select(i => new VotesReportResponse.Piece
																					{
																						Try = i.Try,
																						Signature = Hex.ToHexString(i.Signature),
																						Generator = i.Generator
																					})
																					.ToArray()}; 
	
						case QueryResourceCall c:
							lock(Sun.Lock)
								return Sun.QueryResource(c.Query);
	
						case AddReleaseCall c:
							lock(Sun.Lock)
							{
								var m = new Manifest();
								m.Read(new BinaryReader(new MemoryStream(c.Manifest)));
								
								var h = Sun.Zone.Cryptography.HashFile(m.Bytes);
								
								lock(Sun.Resources.Lock)
								{
									Sun.Resources.Add(c.Release, h);
	
									Sun.Resources.WriteFile(c.Release, h, Package.ManifestFile, 0, c.Manifest);
	
									if(c.Complete != null)
									{
										Sun.Resources.WriteFile(c.Release, h, Package.CompleteFile, 0, c.Complete);
									}
									if(c.Incremental != null)
									{
										Sun.Resources.WriteFile(c.Release, h, Package.IncrementalFile, 0, c.Incremental);
									}
								
									Sun.Resources.SetLatest(c.Release, h);
								}
							}
	
							break;
	
						//case DownloadReleaseCall c:
						//	lock(Core.Lock)
						//		Core.DownloadRelease(c.Release, Workflow);
						//	break;
	
						case PackageStatusCall c:
							lock(Sun.Packages.Lock)
								return Sun.Packages.GetStatus(c.Release, c.Limit);

						case InstallPackageCall c:
							lock(Sun.Packages.Lock)
								Sun.Packages.Install(c.Release, Workflow);
							break;

						case GenerateAnalysisReportCall c:
							lock(Sun.Lock)
							{	
								Sun.Analyses.AddRange(c.Results.Select(i => new Analysis {Resource = i.Key, Result = i.Value}));
							}
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

					lock(Sun.Lock)
						respondjson(rs);
				}
				else
				{
					var r = execute(call);
	
					if(r != null)
					{
						lock(Sun.Lock)
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
