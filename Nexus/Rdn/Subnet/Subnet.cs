using System.Text.RegularExpressions;

namespace Uccs.Rdn;

public enum OutTransactionStatus : byte
{
	None, Sent, Confirmed
}

public class Subnet : IBinarySerializable, ITableEntry
{
	public const int			NameLengthMin = 1;
	public const int			NameLengthMax = 256;
	public const int			PeersMaximum = 1000;
	public const int			RootHashLengthMaximum = 4096;

	public AutoId				Id { get; set; }
	public string				Name { get; set; }
	public int					InNonce { get; set; }
	public Endpoint[]			Peers { get; set; }
	public Snp					Client { get; set; }
	public byte[]				OutHash { get; set; }
	public OutTransactionStatus	OutStatus { get; set; }

	public EntityId				Key => Id;
	public bool					Deleted { get; set; }
	Mcv							Mcv;

	public Subnet()
	{
	}

	public Subnet(Mcv chain)
	{
		Mcv = chain;
	}

	public override string ToString()
	{
		return $"{Name}, {Id}";
	}

	public object Clone()
	{
		return	new Subnet(Mcv)
				{
					Id = Id,
					Name = Name,
					InNonce = InNonce,
					Peers = Peers,
					Client = Client,
					OutHash = OutHash,
					OutStatus = OutStatus,
				};
	}

	public static bool Valid(string name)
	{
		if(name == null)
			return false;

		if(name.Length < NameLengthMin || name.Length > NameLengthMax)
			return false;

		if(Regex.Match(name, $@"^[a-z0-9]+$").Success == false)
			return false;

		return true;
	}

	public static bool Valid(TransactionNna state)
	{
		return state.Peers.Length > PeersMaximum;
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteASCII(Name);
		writer.Write7BitEncodedInt(InNonce);
		writer.Write(Peers);
		writer.Write(Client);
		writer.Write(OutHash);
		writer.Write(OutStatus);
	}

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<AutoId>();
		Name		= reader.ReadASCII();
		InNonce		= reader.Read7BitEncodedInt();
		Peers		= reader.ReadArray<Endpoint>();
		Client		= reader.Read<Snp>();
		OutHash		= reader.ReadHash();
		OutStatus	= reader.Read<OutTransactionStatus>();
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}
