using System.Net;

namespace Uccs.Net;

public class Generator
{
	public AccountAddress	Address { get; set; }
	public AutoId			Id { get; set; }
	public int				Registered { get; set; }
	public Endpoint[]		GraphPpcIPs { get; set; } = [];
	public int				CastingSince { get; set; }
	
	public Peer         	Proxy;

	public override string ToString()
	{
		return $"Address={Address}, CastingSince={CastingSince}, BaseRdcIPs={{{GraphPpcIPs.Length}}}";
	}

  	public virtual void WriteMember(BinaryWriter writer)
 	{
 		writer.Write(Id);
 		writer.Write(Address);
		writer.Write(GraphPpcIPs, i => writer.Write(i));
		writer.Write7BitEncodedInt(CastingSince);
 	}
 
 	public virtual void ReadMember(BinaryReader reader)
 	{
		Id				= reader.Read<AutoId>();
		Address			= reader.Read<AccountAddress>();
		GraphPpcIPs		= reader.ReadArray<Endpoint>();
 		CastingSince	= reader.Read7BitEncodedInt();
	}

  	public virtual void WriteCandidate(BinaryWriter writer)
 	{
 		writer.Write(Id);
 		writer.Write7BitEncodedInt(Registered);
		writer.Write(GraphPpcIPs, i => writer.Write(i));
 	}
 
 	public virtual void ReadCandidate(BinaryReader reader)
 	{
		Id			= reader.Read<AutoId>();
		Registered	= reader.Read7BitEncodedInt();
		GraphPpcIPs	= reader.ReadArray<Endpoint>();
	}

	public virtual Generator Clone()
	{
		var g = new Generator();
	
		Clone(g);
	
		return g;
	}

	public void Clone(Generator generator)
	{
		generator.Address		= Address;
		generator.Id			= Id;
		generator.Registered	= Registered;
		generator.GraphPpcIPs	= GraphPpcIPs;
		generator.CastingSince	= CastingSince;
	}
}
