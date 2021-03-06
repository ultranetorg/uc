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
	public class IPJsonConverter : JsonConverter<IPAddress>
	{
		public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return IPAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class OperationAddress : IBinarySerializable
	{
		public Account	Account { get; set; }
		public int		Id { get; set; }

		public void Read(BinaryReader r)
		{
			Account = r.ReadAccount();
			Id = r.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Account); 
			w.Write7BitEncodedInt(Id);
		}
	}

	public abstract class RpcCall
	{
		public string			Version { get; set; }
		public string			AccessKey { get; set; }

		[JsonIgnore]
		public abstract bool	Private { get; }

		public static string NameOf<C>() => NameOf(typeof(C));
		public static string NameOf(Type type) => type.Name.Remove(type.Name.IndexOf("Call"));
	}

	public class ExitCall : RpcCall
	{
		public override bool	Private => true;
		public string			Reason { get; set; }
	}

	public class StatusCall : RpcCall
	{
		public override bool	Private => false;
		public int				Limit  { get; set; }
	}

	public class GetStatusResponse
	{
		public IEnumerable<string>	Log {get; set;}
		public IEnumerable<string>	Rounds {get; set;}
		public IEnumerable<string>	InfoFields {get; set;}
		public IEnumerable<string>	InfoValues {get; set;}
	}

	public enum MassTransactionCommand
	{
		Start, Stop
	}

	public class RunNodeCall : RpcCall
	{
		public override bool	Private => true;
	}

	public class AddWalletCall : RpcCall
	{
		public override bool	Private => true;
		public Account			Account {get; set;}
		public byte[]			Wallet {get; set;}
	}

	public class UnlockWalletCall : RpcCall
	{
		public override bool	Private => true;
		public Account			Account {get; set;}
		public string			Password {get; set;}
	}

	public class SetGeneratorCall : RpcCall
	{
		public override bool	Private => true;
		public Account			Account {get; set;}
	}

	public class TransferUntCall : RpcCall
	{
		public override bool	Private => true;
		public Account			From {get; set;}
		public Account			To {get; set;}
		public Coin				Amount {get; set;}
	}
}
