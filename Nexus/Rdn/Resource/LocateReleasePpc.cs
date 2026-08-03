using System.Net;

namespace Uccs.Rdn;

public class LocateReleasePpc : RdnPpc<LocateReleasePpr>
{
	public Urr	Address { get; set; }
	public int	Count { get; set; }

	public override Result Execute()
	{
		lock(Mcv.Lock)
			RequireMember();

		lock(Node.SeedHub.Lock)
			return new LocateReleasePpr {Seeders = Node.SeedHub.Locate(this)}; 
	}

	public override void Read(Reader reader)
	{
		Address = reader.ReadVirtual<Urr>();
		Count	= reader.Read7BitEncodedInt();
	}

	public override void Write(Writer writer)
	{
		writer.WriteVirtual(Address);
		writer.Write7BitEncodedInt(Count);
	}
}
	
public class LocateReleasePpr : Result
{
	public Endpoint[]	Seeders { get; set; }

	public override void Read(Reader reader)
	{
		Seeders = reader.ReadArray<Endpoint>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Seeders);
	}
}
