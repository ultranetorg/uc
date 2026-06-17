namespace Uccs.Rdn;

public enum DnsRecordType : byte
{
	A, AAAA,CNAME, MX, TXT, NS, SOA, PTR, SRV, CAA
}

public class DnsRecord : IBinarySerializable
{
	public const int				ValueLengthMaximum = 255;

	public DnsRecordType			Type {get; set;}
	public string					Value {get; set;}
	public int						TTL {get; set;}

  	public void Write(Writer writer)
 	{
 		writer.Write(Type);
		writer.WriteUtf8(Value);
		writer.Write7BitEncodedInt(TTL);
 	}
 
 	public void Read(Reader reader)
 	{
		Type		= reader.Read<DnsRecordType>();
		Value		= reader.ReadUtf8();
		TTL			= reader.Read7BitEncodedInt();
	}

	public override string ToString()
	{
		return $"Type={Type}, Value={Value}, TTL={TTL}";
	}
}
