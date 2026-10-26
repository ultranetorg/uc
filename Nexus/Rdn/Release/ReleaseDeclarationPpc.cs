namespace Uccs.Rdn;

public class ReleaseDeclarationPpc : RdnPpc<ReleaseDeclarationPpr>
{
	public ReleaseDeclaration[]	Resources { get; set; }

	public override Result Execute()
	{
		lock(Node.Mcv.Lock)
			RequireMember();

		lock(Node.SeedHub.Lock)
			return new ReleaseDeclarationPpr {Results = Node.SeedHub.ProcessIncoming(Peer.EP, Resources).ToArray()};
	}

	public override void Read(Reader reader)
	{
		Resources = reader.ReadArray<ReleaseDeclaration>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Resources);
	}
}

public enum DeclarationResult : byte
{
	None, Accepted, ResourceNotFound, Rejected, NotRelease, NotNearest, 
}

public class ReleaseDeclarationResult : IBinarySerializable
{
	public Urn					Address { get; set; }
	public DeclarationResult	Result { get; set; }	

	public ReleaseDeclarationResult()
	{
	}

	public ReleaseDeclarationResult(Urn address, DeclarationResult result)
	{
		Address = address;
		Result = result;
	}

	public void Write(Writer writer)
	{
		writer.Write(Address);
		writer.Write(Result);
	}

	public void Read(Reader reader)
	{
		Address = reader.Read<Urn>();
		Result = reader.Read<DeclarationResult>();
	}
}

public class ReleaseDeclarationPpr : Result
{
	public ReleaseDeclarationResult[]	Results { get; set; }

	public override void Read(Reader reader)
	{
		Results = reader.ReadArray<ReleaseDeclarationResult>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Results);
	}
}
