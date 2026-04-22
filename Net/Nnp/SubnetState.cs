namespace Uccs.Net;

public class SubnetState : IBinarySerializable
{
	public byte[]		RootHash { get; set; }
	public Endpoint[]	Peers { get; set; }

	public byte[]		Hash => _Hash ??= Cryptography.Hash((this as IBinarySerializable).Raw);
	byte[]				_Hash;

	public void Read(BinaryReader reader)
	{
		RootHash = reader.ReadBytes();
		Peers	 = reader.ReadArray<Endpoint>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteBytes(RootHash);
		writer.Write(Peers);
	}

	public override string ToString()
	{
		return $"RootHash={RootHash.ToHex()}, Peers={Peers.Length}";
	}
}
