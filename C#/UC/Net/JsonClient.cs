using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace UC.Net
{
	public class JsonClient
	{
		HttpClient			HttpClient;
		public string		Address;
		string				Key;
		public int			Failures;

		public static JsonSerializerOptions Options;
		
		static JsonClient()
		{
			Options = new JsonSerializerOptions{};

			//Options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

			Options.IgnoreReadOnlyProperties = true;

			Options.Converters.Add(new CoinJsonConverter());
			Options.Converters.Add(new AccountJsonConverter());
			Options.Converters.Add(new IPJsonConverter());
			Options.Converters.Add(new ChainTimeJsonConverter());
			Options.Converters.Add(new ReleaseAddressJsonConverter());
		}

		public JsonClient(HttpClient http, string apiurl, string apikey)
		{
			HttpClient = http;
			Address = apiurl;
			Key = apikey;
		}

		HttpResponseMessage Post(RpcCall request) 
		{
			request.Version = Core.Versions.First().ToString();
			request.AccessKey = Key;

			var c = JsonSerializer.Serialize(request, request.GetType(), Options);

			var m = new HttpRequestMessage(HttpMethod.Get, Address + "/" + RpcCall.NameOf(request.GetType()));

			m.Content = new StringContent(c, Encoding.UTF8, "application/json");

			return HttpClient.Send(m);
		}

		public Rp Send<Rp>(RpcCall request)
		{
			var cr = Post(request);

			if(cr.StatusCode != System.Net.HttpStatusCode.OK)
				throw new RpcException(cr.Content.ReadAsStringAsync().Result);

			try
			{
				return JsonSerializer.Deserialize<Rp>(cr.Content.ReadAsStringAsync().Result, Options);
			}
			catch(Exception ex)
			{
				throw new RpcException("Response deserialization failed", ex);
			}
		}
		
		public void Send(RpcCall request)
		{
			var cr = Post(request);
			
			if(cr.StatusCode != System.Net.HttpStatusCode.OK)
				throw new RpcException(cr.StatusCode.ToString());
		}

		public UntTransfer						Send(TransferUntCall call) => Send<UntTransfer>(call);
		public GetTransactionsStatusResponse	Send(GetTransactionsStatusCall call) => Send<GetTransactionsStatusResponse>(call);
		public GetMembersResponse				Send(GetMembersCall call) => Send<GetMembersResponse>(call);
		public NextRoundResponse				Send(NextRoundCall call) => Send<NextRoundResponse>(call);
		public LastTransactionIdResponse		Send(LastTransactionIdCall call) => Send<LastTransactionIdResponse>(call);
		public DelegateTransactionsResponse		Send(DelegateTransactionsCall call) => Send<DelegateTransactionsResponse>(call);
		public GetStatusResponse				Send(GetStatusCall call) => Send<GetStatusResponse>(call);
		public Operation						Send(LastOperationCall call)
		{
			var r = Send<LastOperationResponse>(call);

			if(r.Operation != null)
			{
				var o = Type.GetType(GetType().Namespace + "." + call.Type).GetConstructor(new Type[0]).Invoke(new object[0]) as Operation;
				o.Read(new BinaryReader(new MemoryStream(r.Operation)));
				return o;
			} 
			else
				return null;
		}
		public AuthorInfo						Send(AuthorInfoCall call) => Send<AuthorInfo>(call);
		public AccountInfo						Send(AccountInfoCall call) => Send<AccountInfo>(call);
	}
}
