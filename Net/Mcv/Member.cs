using System.Net;

namespace Uccs.Net;

public class Member : IBinarySerializable
{
	public AutoId			Generator { get; set; }
	public AutoId			Beneficiary { get; set; }
	public int				Since { get; set; }
	public int				Till { get; set; }
	public Endpoint[]		GraphPpiEndpoints { get; set; } = [];

	public override string ToString()
	{
		return $"{nameof(Generator)}={Generator}, {nameof(Beneficiary)}={Beneficiary}, {nameof(Since)}={Since}, {nameof(Till)}={Till}, {nameof(GraphPpiEndpoints)}={{{GraphPpiEndpoints.Length}}}";
	}

  	public void WriteBase(Writer writer)
 	{
 		writer.Write(Generator);
 		writer.Write(Beneficiary);
		writer.Write(GraphPpiEndpoints);
		writer.Write7BitEncodedInt(Since);
		writer.Write7BitEncodedInt(Till);
 	}
 
 	public void ReadBase(Reader reader)
 	{
		Generator			= reader.Read<AutoId>();
		Beneficiary			= reader.Read<AutoId>();
		GraphPpiEndpoints	= reader.ReadArray<Endpoint>();
 		Since				= reader.Read7BitEncodedInt();
 		Till				= reader.Read7BitEncodedInt();
	}

  	public virtual void Write(Writer writer)
 	{
 		WriteBase(writer);
 	}
 
 	public virtual void Read(Reader reader)
 	{
		ReadBase(reader);
	}

  	public virtual void WriteCandidate(Writer writer)
 	{
 		writer.Write(Generator);
 		writer.Write(Beneficiary);
		writer.Write(GraphPpiEndpoints, i => writer.Write(i));
 	}
 
 	public virtual void ReadCandidate(Reader reader)
 	{
		Generator			= reader.Read<AutoId>();
		Beneficiary			= reader.Read<AutoId>();
		GraphPpiEndpoints	= reader.ReadArray<Endpoint>();
	}

	public virtual Member Clone()
	{
		var g = new Member();
	
		Clone(g);
	
		return g;
	}

	public void Clone(Member generator)
	{
		generator.Generator			= Generator;
		generator.Beneficiary		= Beneficiary;
		generator.GraphPpiEndpoints	= GraphPpiEndpoints;
		generator.Since				= Since;
		generator.Till				= Till;
	}
}
