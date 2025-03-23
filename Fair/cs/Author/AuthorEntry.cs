namespace Uccs.Fair;

public class AuthorEntry : Author, ITableEntry
{
	public BaseId			Key => Id;
	Mcv						Mcv;
	public bool				Deleted { get; set; }
	
	public EntityId[]		Products { get; set; }
	public EntityId[]		Sites { get; set; }

	public AuthorEntry()
	{
	}

	public AuthorEntry(Mcv chain)
	{
		Mcv = chain;
	}

	public override string ToString()
	{
		return $"{Id}, Owners={Owners}, Expiration={Expiration}";
	}

	public AuthorEntry Clone()
	{
		var a = new AuthorEntry(Mcv)   {Id					= Id,
										Nickname			= Nickname,
										Title				= Title,
										Owners				= Owners,

										Expiration			= Expiration,
										Space				= Space,
										Spacetime			= Spacetime,
										ModerationReward	= ModerationReward,

										Products			= Products,
										Sites				= Sites
										};

		((IEnergyHolder)this).Clone(a);

		return a;
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
		
		writer.Write(Products);
		writer.Write(Sites);
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
		
		Products = reader.ReadArray<EntityId>();
		Sites = reader.ReadArray<EntityId>();
	}

	public void WriteMore(BinaryWriter w)
	{
	}

	public void ReadMore(BinaryReader r)
	{
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}
