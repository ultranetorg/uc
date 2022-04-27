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

	public class RpcException : Exception
	{
		public RpcException(string msg) : base(msg){ }
		public RpcException(string msg, Exception ex) : base(msg, ex){ }
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

	public class GetStatusCall : RpcCall
	{
		public override bool	Private => true;
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

	//public class MassTransactionCall : RpcCall
	//{
	//	public override bool Private => true;
	//	public string Command { get; set; }
	//	public string From { get; set; }
	//	public string To { get; set; }
	//}

	public class RunCall : RpcCall
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

	public class NextRoundCall : RpcCall
	{
		public override bool Private => false;
	}

	public class NextRoundResponse
	{
		public int NextRoundId  {get; set;}
	}

	public class LastTransactionIdCall : RpcCall
	{
		public override bool Private => false;
		public Account Account {get; set;}
	}

	public class LastTransactionIdResponse
	{
		public int Id {get; set;}
	}

	public class LastOperationCall : RpcCall
	{
		public override bool	Private => false;
		public Account			Account {get; set;}
		public string			Type {get; set;}
	}

	public class LastOperationResponse
	{
		public byte[] Operation {get; set;}
	}

	public class DelegateTransactionsCall : RpcCall
	{
		public override bool	Private => false;
		public byte[]			Data {get; set;}
	}

	public class DelegateTransactionsResponse
	{
		public IEnumerable<byte[]> Accepted { get; set; }
	}

	public class GetMembersCall : RpcCall
	{
		public override bool Private => false;
	}

	public class GetMembersResponse
	{
		public IEnumerable<Peer> Members { get; set; }
	}

	public class GetTransactionsStatusCall : RpcCall
	{
		public class Item
		{
			public Account	Account { get; set; }
			public int		Id { get; set; }
		}

		public override bool Private => false;
		public IEnumerable<Item> Transactions { get; set; }
	}

	public class GetTransactionsStatusResponse
	{
		public int LastConfirmedRound { get; set; }

		public class Item
		{
			public Account	Account { get; set; }
			public int		Id { get; set; }
			public bool		Confirmed { get; set; }
			public string	Stage { get; set; }
		}

		public IEnumerable<Item> Transactions { get; set; }
	}

	public class AccountInfoCall : RpcCall
	{
		public override bool	Private => false;
		public bool				Confirmed {get; set;} = false;
		public Account			Account {get; set;}
	}

	public class AuthorInfoCall : RpcCall
	{
		public override bool	Private => false;
		public bool				Confirmed {get; set;} = false;
		public string			Name {get; set;}
	}

	public class ReleaseDeclarationCall : RpcCall, IHashable
	{
		public Account			Signer { get;set; }
		public ReleaseAddress	Address { get;set; }
		public string			Stage { get;set; } // stable, beta, nightly, debug, ...
		public List<string>		Localizations { get;set; }
		public byte[]			Signature { get;set; }

		public bool				Valid => Address.Valid;
		public override bool	Private => false;

		public ReleaseDeclarationCall()
		{
		}

		public ReleaseDeclarationCall(PrivateAccount signer, ReleaseAddress address, string stage, List<string> locs)
		{
			Signer = signer;
			Address = address;
			Stage = stage;
			Localizations = locs;

			Signature = Cryptography.Current.Sign(signer, this);
		}
						
		public void HashWrite(BinaryWriter w)
		{
			w.Write(Signer);
			w.Write(Address);
			w.WriteUtf8(Stage);
			w.Write(Localizations, i => w.WriteUtf8(i));
		}
	}

	public class ReleaseRequestCall :  RpcCall
	{
		public ReleaseRequest	Request { get; set; }

		public bool				Valid => Request.Valid;
		public override bool	Private => false;
	}
}
