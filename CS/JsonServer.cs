using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Blake2Fast;

namespace Uccs;

public abstract class Apc
{
	public const string		CredentialsKeyword = "credentials";
	public const string		Postfix = "Apc";
	public static string	NameOf(Type type) => type.Name.Remove(type.Name.LastIndexOf(Postfix));
	public static byte[]	HashifyAdminPassword(string password) => Blake2b.ComputeHash(32, Encoding.UTF8.GetBytes(password));

	public int				Timeout {get; set;} = System.Threading.Timeout.Infinite;
	public int				Limit { get; set; }
}

public class ApiSettings : Settings
{
	public string			LocalAddress { get; set; }
	public string			PublicAddress { get; set; }
	public byte[]			AdminPasswordHash { get; set; }

	public ApiSettings() : base(XonTextValueSerializator.Default)
	{
	}
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

public class JsonConfiguration
{
	protected JsonSerializerOptions	Options;

	public JsonConfiguration()
	{
		Options = CreateOptions();
	}

	public static JsonSerializerOptions CreateOptions()
	{
		var o = new JsonSerializerOptions {};
		
		o.IgnoreReadOnlyProperties = true;

		o.Converters.Add(new IPJsonConverter());
		o.Converters.Add(new VersionJsonConverter());
		o.Converters.Add(new XonJsonConverter());
		o.Converters.Add(new BigIntegerJsonConverter());
		o.Converters.Add(new JsonStringEnumConverter());

		return o;
	}
}

public abstract class JsonServer
{
	HttpListener							Listener;
	Thread									Thread;
	ApiSettings								Settings;
	public Flow								Flow;
	protected JsonSerializerOptions			Options;
	Dictionary<string, ConstructorInfo>		Calls = [];
	protected List<string>					Restricted = [];
	protected const string					All = "*";

	protected abstract object				Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);
	protected abstract Type					Create(string call);

	public JsonServer(ApiSettings settings, JsonSerializerOptions options, Flow flow)
	{
		Settings	= settings;
		Options		= options;
		Flow		= flow.CreateNested("JsonServer", new Log());

		if(Flow.Log != null && Flow.WorkDirectory != null)
		{
			new FileLog(Flow.Log, GetType().Name, Flow.WorkDirectory, Flow);
		}

		Flow.Log?.Report(this, "Listening ", settings.LocalAddress);
		if(settings.PublicAddress != null)
			Flow.Log?.Report(this, "Listening ", settings.PublicAddress);

		Thread = new Thread(() =>	{ 
										try
										{
											Listener = new HttpListener();

											Listener.Prefixes.Add(settings.LocalAddress + "/");
											if(settings.PublicAddress != null)
												Listener.Prefixes.Add(settings.PublicAddress + "/");

											if(Debugger.IsAttached)
											{
												Listener.TimeoutManager.DrainEntityBody			= Timeout.InfiniteTimeSpan;
												Listener.TimeoutManager.EntityBody				= Timeout.InfiniteTimeSpan;
												Listener.TimeoutManager.HeaderWait				= Timeout.InfiniteTimeSpan;
												Listener.TimeoutManager.IdleConnection			= Timeout.InfiniteTimeSpan;
												Listener.TimeoutManager.RequestQueue			= Timeout.InfiniteTimeSpan;
												Listener.TimeoutManager.MinSendBytesPerSecond	= 0;
											}

											Listener.Start();
					
											Flow.Log?.Report(this, "Started");

											while(Flow.Active)
											{
												ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessRequest), Listener.GetContext()); 
											}
										}
										catch(SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
										{
											Flow.Log?.ReportError(this, "Listener Thread Error", ex);
											Listener = null;
										}
										catch(HttpListenerException ex) when (ex.NativeErrorCode == 995 || ex.NativeErrorCode == 500)
										{
											Flow.Log?.ReportError(this, "Listener Thread Error", ex);
											Listener = null;
										}
										catch(Exception ex) when (!Debugger.IsAttached)
										{
											if(!Listener.IsListening)
												Listener = null;

											Flow.Log?.ReportError(this, "Listener Thread Error", ex);
										}
									});

		Thread.Name = $"{settings.LocalAddress + (settings.PublicAddress != null ? ("/" + settings.PublicAddress) : null)} Aping";
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

	protected void RespondError(HttpListenerResponse response, string mime, string text, HttpStatusCode code)
	{
		try
		{

			var buffer = Encoding.UTF8.GetBytes(text);
						
			response.StatusCode = (int)code;
			response.ContentType = mime;
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
		
		try
		{
            if(rq.HttpMethod == HttpMethod.Options.Method)
            {
                rp.StatusCode = 200;
                rp.Close();
                return;
            }

			var p = Listener.Prefixes.First(i => rq.Url.ToString().StartsWith(i));
			var call = rq.Url.LocalPath.Substring(new Uri(p).LocalPath.Length);

			if(Restricted.Any(i => i.Equals(call, StringComparison.InvariantCultureIgnoreCase)))
			{
				if(Settings.AdminPasswordHash == null)
				{	
					RespondError(rp, "text/plain", "Set admin password first", HttpStatusCode.Unauthorized);
					rp.Close();
					return;
				}
				
				var k = HttpUtility.ParseQueryString(rq.Url.Query).Get(Apc.CredentialsKeyword);

				if(string.IsNullOrWhiteSpace(k) || !Bytes.Equal(Apc.HashifyAdminPassword(k), Settings.AdminPasswordHash))
				{
					RespondError(rp, "text/plain", "Password mismatch", HttpStatusCode.Unauthorized);
					rp.Close();
					return;
				}
			}

            rp.AddHeader("Access-Control-Allow-Origin", "*");
            rp.AddHeader("Access-Control-Allow-Methods", "*");
            rp.AddHeader("Access-Control-Allow-Headers", "*");

			Route(context, call);
		}
		catch(HttpListenerException)
		{
		}
		catch(ObjectDisposedException)
		{ 
		}
//		catch(OperationCanceledException)
//		{
//			RespondError(rp, "text/plain", new NodeException(NodeError.a), HttpStatusCode.UnprocessableEntity);
//		}
		catch(CodeException ex)
		{
			RespondError(rp, "application/json", JsonSerializer.Serialize(ex, Options), HttpStatusCode.UnprocessableEntity);
		}
		catch(JsonException ex)
		{
			RespondError(rp, "text/plain", ex.Message, HttpStatusCode.BadRequest);
		}
		catch(Exception ex) when (Environment.GetEnvironmentVariable("UO_Environment") != "Development")
		{
			RespondError(rp, "text/plain", ex.ToString(), HttpStatusCode.InternalServerError);
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

	public void RespondJson(HttpListenerResponse response, object t)	
	{
		var output = response.OutputStream;
		var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(t, Options));
						
		response.ContentType = "text/json" ;
		response.ContentLength64 = buffer.Length;

		output.Write(buffer, 0, buffer.Length);
	}

	protected virtual void Route(HttpListenerContext context, string call)
	{
		var rq = context.Request;
		var rp = context.Response;

		if(call == "Ping")
		{
			rp.StatusCode = (int)HttpStatusCode.OK;
			RespondJson(context.Response, new Pong {Status = "OK"});
			rp.Close();
			return;
		}

		ConstructorInfo constuctor;
			
		lock(Calls)
			if(!Calls.TryGetValue(call, out constuctor))
			{
				var t = Type.GetType($"{typeof(JsonServer).Namespace}.{call}{Apc.Postfix}") ?? Create(call + Apc.Postfix);

				if(t == null)
				{
					RespondError(rp,  "text/plain", HttpStatusCode.NotFound.ToString(), HttpStatusCode.NotFound);
					rp.Close();
					return;
				}

				Calls[call] = constuctor = t.GetConstructor(new System.Type[]{});
			}

		var c = (rq.ContentLength64 > 0 ? JsonSerializer.Deserialize(rq.InputStream, constuctor.DeclaringType, Options) : constuctor.Invoke(null)) as Apc;

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
				var t = Create(i.Name + Apc.Postfix);
				rs.Add(execute(JsonSerializer.Deserialize(i.Call, t, Options) as Apc));
			}

			RespondJson(context.Response, rs);
		}
		else
		{
			var r = execute(c);

			//if(r != null)
			{
				RespondJson(context.Response, r);
			}
		}
	}
}

