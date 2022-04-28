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
		Dispatcher		Dispatcher;
		HttpListener	Listener;
		Thread			Thread;

		Log				Log  => Dispatcher.Log;
		Settings		Settings => Dispatcher.Settings;
		Vault			Vault => Dispatcher.Vault;
		Roundchain		Chain => Dispatcher.Chain;

		public JsonServer(Dispatcher dispatcher)
		{
			Dispatcher = dispatcher;

			Thread = new Thread(() =>
								{ 
									try
									{
										Listener = new HttpListener();
	
										var prefixes = new string[] {$"http://{(Settings.IP.ToString() != "0.0.0.0" ? Settings.IP.ToString() : "+")}:{Zone.RpcPort(Settings.Zone)}/"};
			
										foreach(string s in prefixes)
										{
											Listener.Prefixes.Add(s);
										}
	
										Listener.Start();
				
										Log?.Report(this, "Listening started", prefixes[0]);
		
										while(Dispatcher.Working)
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

										Dispatcher.Stop(MethodBase.GetCurrentMethod(), ex);
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

		void ProcessRequest(object o)
		{
			var context = o as HttpListenerContext;

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

				if(call.Private && (string.IsNullOrWhiteSpace(Settings.Rpc.AccessKey) || call.AccessKey != Settings.Rpc.AccessKey))
				{
					rp.StatusCode = (int)HttpStatusCode.Unauthorized;
					rp.Close();
					return;
				}

				rp.StatusCode = (int)HttpStatusCode.OK;

				lock(Dispatcher.Lock)
					switch(call)
					{
						case RunCall e:
							Dispatcher.RunNode();
							break;

						case AddWalletCall e:
							Vault.AddWallet(e.Account, e.Wallet);
							break;

						case UnlockWalletCall e:
							Vault.Unlock(e.Account, e.Password);
							break;
	
						case SetGeneratorCall e:
							Dispatcher.Settings.Generator = Vault.GetPrivate(e.Account).Key.GetPrivateKey();
							Log?.Report(this, "Generator is set", e.Account.ToString());
							break;

						case TransferUntCall e:
							respondjson(Dispatcher.Enqueue(new UntTransfer(Vault.GetPrivate(e.From), e.To, e.Amount)));
							Log?.Report(this, "TransferUnt received", $"{e.From} -> {e.Amount} -> {e.To}");
							break;
	
						case GetStatusCall s:
							respondjson(new GetStatusResponse {	Log			= Log?.Messages.TakeLast(s.Limit).Select(i => i.ToString()), 
																Rounds		= Chain.Rounds.Take(s.Limit).Reverse().Select(i => i.ToString()), 
																InfoFields	= Dispatcher.Info[0].Take(s.Limit), 
																InfoValues	= Dispatcher.Info[1].Take(s.Limit) });
							break;
							
						case NextRoundCall e:
							if(Dispatcher.Synchronization != Synchronization.Synchronized)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
							{
								var r = Dispatcher.GetNextAvailableRound();
								
								if(r != null)
									respondjson(new NextRoundResponse {NextRoundId = r.Id});
								else
									responderror("Next round not available");
							}
									
							break;
							
						case LastTransactionIdCall e:
							if(Dispatcher.Synchronization != Synchronization.Synchronized)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
							{
								var t = Dispatcher.Chain.Accounts.FindLastTransaction(e.Account, i => i.Successful);
							
								respondjson(new LastTransactionIdResponse {Id = t != null ? t.Id : -1});
							}
									
							break;
							
						case LastOperationCall e:
						{
							if(Dispatcher.Synchronization != Synchronization.Synchronized)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
							{
								var l = Dispatcher.Chain.Accounts.FindLastOperation(e.Account, i => i.GetType().Name == e.Type);
							
								if(l != null)
								{
									var s = new MemoryStream();
									var w = new BinaryWriter(s);
									l.Write(w);
									respondjson(new LastOperationResponse {Operation = s.ToArray()});
								} 
								else
									respondjson(new LastOperationResponse {Operation = null});
							}
									
							break;
						}	
						
						case DelegateTransactionsCall e:
							if(Dispatcher.Synchronization != Synchronization.Synchronized || Dispatcher.Generator == null)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
							{
								IEnumerable<Transaction> txs = new Transaction[0];

								var cd = Chain.Accounts.FindLastOperation<CandidacyDeclaration>(Dispatcher.Generator);
		
								txs = Dispatcher.Read(new MemoryStream(e.Data), r =>{
																						return  new Transaction(Settings)
																								{
																									Member = Dispatcher.Generator
																								};
																					});
		
								var accep = Dispatcher.ProcessIncoming(txs);

								respondjson(new DelegateTransactionsResponse {Accepted = accep.Select(i => i.Signature)});
		
								Log?.Report(this, "Incoming transaction(s)", $"Received={txs.Count()} Accepted={accep.Count()} from {context.Request.RemoteEndPoint}");
							}

							break;

						case GetMembersCall e:
							if(Chain != null)
							{
								if(Dispatcher.Synchronization == Synchronization.Synchronized)
									respondjson(new GetMembersResponse {Members = Chain.Members});
								else
									rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							}
							else
							{
								if(Dispatcher.Members.Any())
									respondjson(new GetMembersResponse {Members = Dispatcher.Members});
								else
									rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							}

							break;

						case GetTransactionsStatusCall c:
							if(Dispatcher.Synchronization != Synchronization.Synchronized)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
								respondjson(new GetTransactionsStatusResponse
											{
												LastConfirmedRound = Chain.LastConfirmedRound.Id,
												Transactions = c.Transactions	.Select(t => Dispatcher.Chain.Accounts.FindLastTransaction(t.Account, i => i.Successful && i.Id == t.Id))
																				.Where(i => i != null)
																				.Select(i => new GetTransactionsStatusResponse.Item{Account		= i.Signer,
																																	Id			= i.Id,
																																	Confirmed	= i.Payload.Round.Confirmed,
																																	Stage		= i.Stage.ToString()})
											});

							break;

						case AccountInfoCall c:
							if(Dispatcher.Synchronization != Synchronization.Synchronized)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
							{
								var ai = Chain.GetAccountInfo(c.Account, c.Confirmed);
								
								if(ai != null)
									respondjson(ai);
								else
									responderror("Account not found");
							}
							break;

						case AuthorInfoCall c:
							if(Dispatcher.Synchronization != Synchronization.Synchronized)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
							{
								var ai = Chain.GetAuthorInfo(c.Name, c.Confirmed);
								
								if(ai != null)
									respondjson(ai);
								else
									responderror("Author not found");
							}
							break;

						case DeclareReleaseCall c:
							if(Dispatcher.Synchronization != Synchronization.Synchronized)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
							{
								Dispatcher.DecalreRelease(c.Declaration);
							}
							break;

						case QueryReleaseCall c:
							if(Dispatcher.Synchronization != Synchronization.Synchronized)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
							{
								var ai = Dispatcher.QueryRelease(c.Query);
								
								respondjson(ai);
							}
							break;

						case DownloadReleaseCall c:
							if(Dispatcher.Synchronization != Synchronization.Synchronized)
								rp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
							else
							{
								var ai = Dispatcher.DownloadRelease(c.Request);
								
								respondbinary(ai);
							}
							break;

						case ExitCall e:
							rp.Close();
							Dispatcher.Stop("RPC call");
							return;
	
						default:
							rp.StatusCode = (int)HttpStatusCode.NotFound;
							break;
					}
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
