using System.Text.RegularExpressions;

namespace Uccs.Rdn;

public enum SubnetStatus
{
	None,
	Initialized,
	BlockRecieved,
	BlockSent
}

public class Subnet : IBinarySerializable, ITableEntry
{
	public const int				NameLengthMin = 1;
	public const int				NameLengthMax = 256;
	public const int				PeersMaximum = 1000;
	public const int				RootHashLengthMaximum = 4096;

	public AutoId					Id { get; set; }
	public string					Address { get; set; }
	public SubnetState				State { get; set; }
	public byte[]					StateHash { get; set; }
	public Snp						Client { get; set; }

	public EntityId					Key => Id;
	public bool						Deleted { get; set; }
	Mcv								Mcv;

	public Subnet()
	{
	}

	public Subnet(Mcv chain)
	{
		Mcv = chain;
	}

	public override string ToString()
	{
		return $"{Address}, {Id}";
	}

	public object Clone()
	{
		return	new Subnet(Mcv)
				{
					Id = Id,
					Address = Address,
					State = State,
					StateHash = StateHash,
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

	public static bool Valid(SubnetState state)
	{
		return state.RootHash.Length > RootHashLengthMaximum || state.Peers.Length > PeersMaximum;
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
		writer.WriteASCII(Address);

		writer.Write(State);
		writer.Write(StateHash);
		writer.Write(Client);
	}

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<AutoId>();
		Address		= reader.ReadASCII();

		State		= reader.Read<SubnetState>();
		StateHash	= reader.ReadHash();
		Client		= reader.Read<Snp>();
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}
