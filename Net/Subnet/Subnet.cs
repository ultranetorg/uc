using System.Text.RegularExpressions;

namespace Uccs.Net;

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
	public Snp					Client { get; set; }
	public Endpoint[]			Peers { get; set; }
	public int					InNonce { get; set; }
	public int					OutNonce { get; set; }
	public IccpOperation[]		OutOperations { get; set; }
	public byte[]				OutHash { get; set; }
	public OutTransactionStatus	OutStatus { get; set; }

	public EntityId				Key => Id;
	public bool					Deleted { get; set; }
	Mcv							Mcv;

	public Subnet()
	{
	}

	public Subnet(Mcv mcv)
	{
		Mcv = mcv;
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
					Client = Client,
					Peers = Peers,
					InNonce = InNonce,
					OutNonce = OutNonce,
					OutOperations = OutOperations,
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
		writer.Write(Client);
		writer.Write(Peers);
		writer.Write7BitEncodedInt(InNonce);
		writer.Write7BitEncodedInt(OutNonce);
		writer.Write(OutOperations);
		writer.Write(OutHash);
		writer.Write(OutStatus);
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<AutoId>();
		Name			= reader.ReadASCII();
		Client			= reader.Read<Snp>();
		Peers			= reader.ReadArray<Endpoint>();
		InNonce			= reader.Read7BitEncodedInt();
		OutNonce		= reader.Read7BitEncodedInt();
		OutOperations	= reader.ReadArray<IccpOperation>();
		OutHash			= reader.ReadHash();
		OutStatus		= reader.Read<OutTransactionStatus>();
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}
