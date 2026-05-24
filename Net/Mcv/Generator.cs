using System.Net;

namespace Uccs.Net;

public class Generator
{
	public AutoId			User { get; set; }
	public Endpoint[]		GraphPpiEndpoints { get; set; } = [];
	public int				Since { get; set; }
	public int				Till { get; set; }
	
	public Peer         	Proxy;

	public override string ToString()
	{
		return $"Id={User}, Since={Since}, Till={Till}, BaseRdcIPs={{{GraphPpiEndpoints.Length}}}";
	}

  	public virtual void WriteMember(Writer writer)
 	{
 		writer.Write(User);
		writer.Write(GraphPpiEndpoints);
		writer.Write7BitEncodedInt(Since);
		writer.Write7BitEncodedInt(Till);
 	}
 
 	public virtual void ReadMember(Reader reader)
 	{
		User			= reader.Read<AutoId>();
		GraphPpiEndpoints		= reader.ReadArray<Endpoint>();
 		Since			= reader.Read7BitEncodedInt();
 		Till			= reader.Read7BitEncodedInt();
	}

  	public virtual void WriteCandidate(Writer writer)
 	{
 		writer.Write(User);
		writer.Write(GraphPpiEndpoints, i => writer.Write(i));
 	}
 
 	public virtual void ReadCandidate(Reader reader)
 	{
		User		= reader.Read<AutoId>();
		GraphPpiEndpoints	= reader.ReadArray<Endpoint>();
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
		generator.GraphPpiEndpoints	= GraphPpiEndpoints;
		generator.Since			= Since;
		generator.Till			= Till;
	}
}
