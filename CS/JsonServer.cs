using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Uccs;

public abstract class Apc
{
	public const string		AccessKey = "accesskey";
	public const string		Postfix = "Apc";
	public static string	NameOf(Type type) => type.Name.Remove(type.Name.IndexOf(Postfix));

	public int				Timeout {get; set;} = System.Threading.Timeout.Infinite;
}

public class ApiSettings : Settings
{
	public ApiSettings() : base(XonTextValueSerializator.Default)
	{
	}

	public string	AccessKey { get; set; }
	public string	ListenAddress { get; set; }
}

public class PingApc : Apc
{
}

public class Pong
{
	public string Status { get; set; }
}

public class BatchApc : Apc
{
	public class Item
	{
		public string	Name { get; set; }
		public dynamic	Call { get; set; }
	}

	public IEnumerable<Item> Calls { get; set; }

	public void Add(Apc call)
	{
		if(Calls == null)
			Calls = new List<Item>();

		(Calls as List<Item>).Add(new Item {Name = Apc.NameOf(call.GetType()), Call = call});
	}
}

public abstract class JsonServer
{
	HttpListener					Listener;
	Thread							Thread;
	ApiSettings						Settings	;
	Flow							Flow;
	JsonSerializerOptions			Options;

	protected abstract object		Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);
	protected abstract Type			Create(string call);

	public JsonServer(ApiSettings settings, JsonSerializerOptions options, Flow workflow)
	{
		Settings	= settings;
		Options		= options;
		Flow		= workflow.CreateNested("JsonServer", new Log());

		//if(profile != null)
		//{
		//	Flow.Log.Reported += m => File.AppendAllText(Path.Combine(profile, "JsonApiServer.log"), m.ToString() + Environment.NewLine);
		//}

		Thread = new Thread(() =>	{ 
										try
										{
											Listener = new HttpListener();
											
											Listener.Prefixes.Add(settings.ListenAddress + "/");

 												//if(ip != null)
 												//{
 												//	Listener.Prefixes.Add($"http://{ip}:{port}/");
 												//}
 												//else
 												//{
 												//	Listener.Prefixes.Add($"http://+:{port}/");
 												//}

									
											Flow.Log?.Report(this, "Listening started", Listener.Prefixes.Last());

											Listener.Start();
					
											while(Flow.Active)
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

											Flow.Log?.ReportError(this, "Listener Thread Error", ex);
										}
									});

		Thread.Name = $"{settings.ListenAddress} Aping";
		Thread.Start();
	}

	public void Stop()
	{
		Flow.Abort();
		Listener?.Stop();
	}

	public void WaitStop()
	{
		Thread?.Join();
	}

	protected void RespondError(HttpListenerResponse response, string t, HttpStatusCode code)
	{
		try
		{

			var buffer = Encoding.UTF8.GetBytes(t);
						
			response.StatusCode = (int)code;
			response.ContentType = "text/plain" ;
			response.ContentLength64 = buffer.Length;
			response.OutputStream.Write(buffer, 0, buffer.Length);
		}
		catch(InvalidOperationException)
		{
		}
		catch(HttpListenerException)
		{
		}
	}

	void ProcessRequest(object obj)
	{
		var context = obj as HttpListenerContext;

		var rq = context.Request;
		var rp = context.Response;

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
			if(!string.IsNullOrWhiteSpace(Settings.AccessKey) && System.Web.HttpUtility.ParseQueryString(rq.Url.Query).Get(Apc.AccessKey) != Settings.AccessKey)
			{
				RespondError(rp, HttpStatusCode.Unauthorized.ToString(), HttpStatusCode.Unauthorized);
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
			
			var t = Type.GetType(typeof(JsonServer).Namespace + '.' + rq.Url.LocalPath.Substring(1) + Apc.Postfix) ?? Create(rq.Url.LocalPath.Substring(1) + Apc.Postfix);

			if(t == null)
			{
				RespondError(rp, HttpStatusCode.NotFound.ToString(), HttpStatusCode.NotFound);
				rp.Close();
				return;
			}

			var reader = new StreamReader(rq.InputStream, rq.ContentEncoding);
			var j = reader.ReadToEnd();
			var c = (j.Any() ? JsonSerializer.Deserialize(j, t, Options) : t.GetConstructor(new System.Type[]{}).Invoke(null)) as Apc;

			rp.StatusCode = (int)HttpStatusCode.OK;

			object execute(Apc call)
			{
				var f = Flow.CreateNested(rq.Url.ToString(), new Log());

				f.CancelAfter(call.Timeout);

				return Execute(call, rq, rp, f);
			}

			if(c is BatchApc b)
			{ 
				var rs = new List<dynamic>();

				foreach(var i in b.Calls)
				{
					t = Create(i.Name + Apc.Postfix);
					rs.Add(execute(JsonSerializer.Deserialize(i.Call, t, Options) as Apc));
				}

				respondjson(rs);
			}
			else
			{
				var r = execute(c);

				//if(r != null)
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
			RespondError(rp, ex.Message, HttpStatusCode.BadRequest);
		}
		catch(Exception ex) /// when (!Debugger.IsAttached)
		{
			RespondError(rp, ex.ToString(), HttpStatusCode.InternalServerError);
			Flow.Log?.ReportError(this, "Request Processing Error", ex);
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
