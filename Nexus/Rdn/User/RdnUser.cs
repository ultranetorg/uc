using System.Collections.Immutable;

namespace Uccs.Rdn;

public class RdnUser : User
{
	public ImmutableList<AutoId>	Domains  { get; set; }
	public ImmutableList<StringId>	DomainNames  { get; set; }

	public RdnUser()
	{
	}

	public RdnUser(Mcv mcv) : base(mcv)
	{
	}

	public override User Clone()
	{
		var a = base.Clone() as RdnUser;

		a.Domains		= Domains;
		a.DomainNames	= DomainNames;

		return a;
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);

		writer.Write(Domains);
		writer.Write(DomainNames, i => writer.Write(i));
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);

		Domains			= reader.ReadImmutableList<AutoId>();
		DomainNames		= reader.ReadImmutableList(() => reader.Read<StringId>());
	}
}
