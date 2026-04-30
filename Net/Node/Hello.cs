using System.Net;

namespace Uccs.Net;

public abstract class Hello
{
	public int[]						Versions;
	public long							Roles;
	public IPAddress					YourIP;
	public ushort						MyPort;
	public bool							Permanent;
	public string						Name;

	public virtual void Write(BinaryWriter w)
	{
		w.Write(Versions, i => w.Write7BitEncodedInt(i));
		w.WriteUtf8(Name);
		w.Write7BitEncodedInt64(Roles);
		w.Write(YourIP);
		w.Write(MyPort);
		w.Write(Permanent);
	}

	public virtual void Read(BinaryReader r)
	{
		Versions	= r.ReadArray(() => r.Read7BitEncodedInt());
		Name		= r.ReadUtf8();
		Roles		= r.Read7BitEncodedInt64();
		YourIP		= r.ReadIPAddress();
		MyPort		= r.ReadUInt16();
		Permanent	= r.ReadBoolean();
	}
}

public class HomoHello : Hello
{
	public string	Net;

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.WriteASCII(Net);
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		Net	= reader.ReadASCII();
	}
}

public class NnHello : Hello
{
	public List<string>	Nets;

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.Write(Nets, writer.WriteASCII);
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		Nets = reader.ReadList(reader.ReadASCII);
	}
}