using System.Net;

namespace Uccs.Net;

public class Generator
{
	public AutoId			User { get; set; }
	public Endpoint[]		GraphPpcIPs { get; set; } = [];
	public int				Since { get; set; }
	
	public Peer         	Proxy;

	public override string ToString()
	{
		return $"Id={User}, CastingSince={Since}, BaseRdcIPs={{{GraphPpcIPs.Length}}}";
	}

  	public virtual void WriteMember(BinaryWriter writer)
 	{
 		writer.Write(User);
		writer.Write(GraphPpcIPs, i => writer.Write(i));
		writer.Write7BitEncodedInt(Since);
 	}
 
 	public virtual void ReadMember(BinaryReader reader)
 	{
		User			= reader.Read<AutoId>();
		GraphPpcIPs		= reader.ReadArray<Endpoint>();
 		Since			= reader.Read7BitEncodedInt();
	}

  	public virtual void WriteCandidate(BinaryWriter writer)
 	{
 		writer.Write(User);
		writer.Write(GraphPpcIPs, i => writer.Write(i));
 	}
 
 	public virtual void ReadCandidate(BinaryReader reader)
 	{
		User		= reader.Read<AutoId>();
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
		generator.User			= User;
		generator.GraphPpcIPs	= GraphPpcIPs;
		generator.Since			= Since;
	}
}
