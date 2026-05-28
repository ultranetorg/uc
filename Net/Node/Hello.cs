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

	public virtual void Write(Writer w)
	{
		w.Write(Versions, w.Write7BitEncodedInt);
		w.WriteUtf8(Name);
		w.Write7BitEncodedInt64(Roles);
		w.Write(YourIP);
		w.Write(MyPort);
		w.Write(Permanent);
	}

	public virtual void Read(Reader r)
	{
		Versions	= r.ReadArray(r.Read7BitEncodedInt);
		Name		= r.ReadUtf8();
		Roles		= r.Read7BitEncodedInt64();
		YourIP		= r.ReadIPAddress();
		MyPort		= r.ReadUInt16();
		Permanent	= r.ReadBoolean();
	}
}
