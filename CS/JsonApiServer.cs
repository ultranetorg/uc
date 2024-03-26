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

namespace Uccs
{
	public abstract class ApiCall
	{
		public static string	NameOf(Type type) => type.Name.Remove(type.Name.IndexOf("Call"));
	}

	public class PingCall : ApiCall
	{
	}

	public class Pong
	{
		public string Status { get; set; }
	}

	public class BatchCall : ApiCall
	{
		public class Item
		{
			public string	Name { get; set; }
			public dynamic	Call { get; set; }
		}

		public IEnumerable<Item> Calls { get; set; }

		public void Add(ApiCall call)
		{
			if(Calls == null)
				Calls = new List<Item>();

			(Calls as List<Item>).Add(new Item {Name = call.GetType().Name.Remove(call.GetType().Name.IndexOf("Call")), Call = call});
		}
	}

	public abstract class JsonApiServer
	{
		HttpListener					Listener;
		Thread							Thread;
		string							AccessKey;
		Workflow						Workflow;
		JsonSerializerOptions			Options;

		protected abstract object		Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow);
		protected abstract Type			Create(string call);

		public JsonApiServer(string profile, IPAddress ip, ushort port, string accesskey, JsonSerializerOptions options, Workflow workflow)
		{
			AccessKey	= accesskey;
			Options		= options;
			Workflow	= workflow.CreateNested("JsonApiServer", new Log());

			if(profile != null)
			{
				Workflow.Log.Reported += m => File.AppendAllText(Path.Combine(profile, "JsonApiServer.log"), m.ToString() + Environment.NewLine);
			}

			Thread = new Thread(() =>	{ 
											try
											{
												Listener = new HttpListener();
	
 												if(ip != null)
 												{
 													Listener.Prefixes.Add($"http://{ip}:{port}/");
 												}
 												else
 												{
 													Listener.Prefixes.Add($"http://+:{port}/");
 												}

										
												Workflow.Log?.Report(this, "Listening started", Listener.Prefixes.Last());

												Listener.Start();
						
												while(Workflow.Active)
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

												Workflow.Log?.ReportError(this, "Listener Thread Error", ex);
											}
										});

			Thread.Name = $"{ip?.GetAddressBytes()[3]} Aping";
			Thread.Start();
		}

		public void Stop()
		{
			Workflow.Abort();
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
	
			void respondjson(object t)	{
											var output = rp.OutputStream;
											var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(t, Options));
							
											rp.ContentType = "text/json" ;
											rp.ContentLength64 = buffer.Length;

											output.Write(buffer, 0, buffer.Length);
										}

// 			void respondbinary(byte[] t){
// 											try
// 											{
// 												rp.ContentType = "application/octet-stream";
// 												rp.OutputStream.Write(t, 0, t.Length);
// 											}
// 											catch(InvalidOperationException)
// 											{
// 											}
// 										}
			
			try
			{
				if(!string.IsNullOrWhiteSpace(AccessKey) && System.Web.HttpUtility.ParseQueryString(rq.Url.Query).Get("accesskey") != AccessKey)
				{
					rp.StatusCode = (int)HttpStatusCode.Unauthorized;
					rp.Close();
					return;
				}

				if(rq.Url.LocalPath.Substring(1) == "Ping")
				{
					rp.StatusCode = (int)HttpStatusCode.OK;
					respondjson(new Pong {Status = "OK"});
					rp.Close();
					return;
				}
				
				var t = Type.GetType(GetType().Namespace + '.' + rq.Url.LocalPath.Substring(1) + "Call") ?? Create(rq.Url.LocalPath.Substring(1) + "Call");

				if(t == null)
				{
					rp.StatusCode = (int)HttpStatusCode.NotFound;
					rp.Close();
					return;
				}

				var reader = new StreamReader(rq.InputStream, rq.ContentEncoding);
				var j = reader.ReadToEnd();
				var c = (j.Any() ? JsonSerializer.Deserialize(j, t, Options) : t.GetConstructor(new System.Type[]{}).Invoke(null)) as ApiCall;

				rp.StatusCode = (int)HttpStatusCode.OK;

				object execute(ApiCall call)
				{
					return Execute(call, rq, rp, Workflow.CreateNested(MethodBase.GetCurrentMethod().Name));
				}

				if(c is BatchCall b)
				{ 
					var rs = new List<dynamic>();

					foreach(var i in b.Calls)
					{
						t = Create(i.Name + "Call");
						rs.Add(execute(JsonSerializer.Deserialize(i.Call, t, Options) as ApiCall));
					}

					respondjson(rs);
				}
				else
				{
					var r = execute(c);
	
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
				Workflow.Log?.ReportError(this, "Request Processing Error", ex);
			}

			try
			{
				rp.Close();
			}
			catch(Exception)
			{
			}
		}
	}
}
