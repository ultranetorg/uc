using System.Net;

namespace Uccs.Net;

public class NnpState : IBinarySerializable
{
	public byte[]		RootHash { get; set; }
	public Endpoint[]	Peers { get; set; }

	public byte[]		Hash => Cryptography.Hash((this as IBinarySerializable).Raw);

	public void Read(BinaryReader reader)
	{
		RootHash = reader.ReadBytes();
		Peers = reader.ReadArray<Endpoint>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteBytes(RootHash);
		writer.Write(Peers);
	}
}
