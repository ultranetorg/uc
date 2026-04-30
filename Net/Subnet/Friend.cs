using System.Text.RegularExpressions;

namespace Uccs.Net;

public enum IccTransferStatus : byte
{
	None, FormedAndPending, Confirmed
}

public class Friend : IBinarySerializable, ITableEntry
{
	public const int			NameLengthMin = 1;
	public const int			NameLengthMax = 256;
	public const int			PeersMaximum = 1000;
	public const int			RootHashLengthMaximum = 4096;

	public AutoId				Id { get; set; }
	public string				Name { get; set; }
	public Snp					Client { get; set; }
	public Endpoint[]			Peers { get; set; }
	public byte[]				LastAcceptedTransfer { get; set; }
	public IccTransfer			LastOutgoingTransfer { get; set; }
	public IccTransferStatus	OutStatus { get; set; }

	public EntityId				Key => Id;
	public bool					Deleted { get; set; }
	Mcv							Mcv;

	public Friend()
	{
	}

	public Friend(Mcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Name}, {Id}";
	}

	public object Clone()
	{
		return	new Friend(Mcv)
				{
					Id = Id,
					Name = Name,
					Client = Client,
					Peers = Peers,
					LastAcceptedTransfer = LastAcceptedTransfer,
					LastOutgoingTransfer = LastOutgoingTransfer,
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

	public static bool Valid(IccTransfer state)
	{
		return true; //state.Peers.Length > PeersMaximum;
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
		writer.Write(LastAcceptedTransfer != null); if(LastAcceptedTransfer != null) writer.Write(LastAcceptedTransfer);
		writer.Write(LastOutgoingTransfer != null); if(LastOutgoingTransfer != null) writer.Write(LastOutgoingTransfer);
		writer.Write(OutStatus);
	}

	public void Read(BinaryReader reader)
	{
		Id						= reader.Read<AutoId>();
		Name					= reader.ReadASCII();
		Client					= reader.Read<Snp>();
		Peers					= reader.ReadArray<Endpoint>();
		if(reader.ReadBoolean())
			LastAcceptedTransfer	= reader.ReadHash();
		if(reader.ReadBoolean())
		{	
			LastOutgoingTransfer = new (null); 
			LastOutgoingTransfer.Read(reader);
		}
		OutStatus				= reader.Read<IccTransferStatus>();
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}
