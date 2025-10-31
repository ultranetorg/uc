using System.Net;

namespace Uccs.Net;

public class NtnState : IBinarySerializable
{
	public byte[] Hash => Cryptography.Hash((this as IBinarySerializable).Raw);

	public class Peer : IBinarySerializable
	{
		public IPAddress	IP { get; set; }
		public ushort		Port { get; set; }

		public void Read(BinaryReader reader)
		{
			IP		= reader.ReadIPAddress();
			Port	= reader.ReadUInt16();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(IP);
			writer.Write(Port);
		}
	}

	public byte[]	State { get; set; }
	public Peer[]	Peers { get; set; }

	public void Read(BinaryReader reader)
	{
		State = reader.ReadBytes();
		Peers = reader.ReadArray<Peer>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteBytes(State);
		writer.Write(Peers);
	}
}
