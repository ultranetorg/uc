using System.Text.RegularExpressions;

namespace Uccs.Rdn;

public enum DomainFlag : byte
{
	None, 
	Free		= 0b_______1, 
	//ChildNet	= 0b__100000, 
}

public enum OwnershipPolicy : byte
{
	None, 
	FullOwnership	= 1, 
	FullFreedom		= 2, 
	///Programmatic	= 0b11111111, 
}

public class Domain : ITableEntry<AutoId>, IBinarySerializable, ISpaceConsumer
{
	public AutoId					Id { get; set; }
	public string					Name { get; set; }
	public AutoId					Owner { get; set; }
	public OwnershipPolicy			OwnershipPolicy { get; set; }

	public long						Space { get; set; }
	public short					Expiration { get; set; }

	public bool						Deleted { get; set; }
	Mcv								Mcv;

	public Domain()
	{
	}

	public Domain(Mcv chain)
	{
		Mcv = chain;
	}

	public override string ToString()
	{
		return $"{Name}, {Id}, {nameof(Owner)}={Owner}, {nameof(Space)}={Space}, {nameof(Expiration)}={Expiration}";
	}

	public object Clone()
	{
		return	new Domain(Mcv)
				{	
					Id				= Id,
					Name			= Name,
					Owner			= Owner,
					OwnershipPolicy	= OwnershipPolicy,
					Space			= Space,
					Expiration		= Expiration,
				};
	}

	public void WriteMain(Writer writer)
	{
		var f = DomainFlag.None;
		
		writer.Write(f);
		writer.WriteASCII(Name);
		writer.Write(Owner);

		(this as ISpaceConsumer).WriteSpaceConsumer(writer);

		if(DomainName.IsSubdomain(Name))
		{
			writer.Write((byte)OwnershipPolicy);
		}
	}

	public void ReadMain(Reader reader)
	{
		var f		= reader.Read<DomainFlag>();
		Name		= reader.ReadASCII();
		Owner		= reader.Read<AutoId>();

		(this as ISpaceConsumer).ReadSpaceConsumer(reader);

		if(DomainName.IsSubdomain(Name))
		{
			OwnershipPolicy = (OwnershipPolicy)reader.ReadByte();
		}
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		WriteMain(writer);
	}

	public void Read(Reader reader)
	{
		Id	= reader.Read<AutoId>();
		ReadMain(reader);
	}

	public void Cleanup(Round lastInCommit)
	{
	}
			
	public static string GetRoot(string name)
	{
		var i = name.LastIndexOf('.');

		return i == -1 ? name : name.Substring(i + 1);
	}
}
