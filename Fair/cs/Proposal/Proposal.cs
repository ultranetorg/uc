namespace Uccs.Fair;

public enum ProposalFlags : byte
{
}

public class ProposalOption : Option
{
	public AutoId[]	Yes { get; set; }

	public ProposalOption()
	{
	}

	public ProposalOption(Option option)
	{
		Title		= option.Title;
		Operation	= option.Operation;
		Yes			= [];
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
 		Yes = reader.ReadArray<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);	
 		writer.Write(Yes);
	}
}


public class Proposal : IBinarySerializable, ITableEntry
{
	public AutoId				Id { get; set; }
	public AutoId				Site { get; set; }
	public AutoId				By { get; set; }
	public Role					As { get; set; }
	public ProposalFlags		Flags { get; set; }
	public AutoId[]				Neither { get; set; }
	public AutoId[]				Abs { get; set; }
	public AutoId[]				Ban { get; set; }
	public AutoId[]				Banish { get; set; }
	public Time					CreationTime { get; set; }
 	public string				Title { get; set; }
 	public string				Text { get; set; }
	public ProposalOption[]		Options { get; set; }
	public AutoId[]				Comments;

	public EntityId				Key => Id;
	public bool					Deleted { get; set; }
	FairMcv						Mcv;
		
    public FairOperationClass	OptionClass => (FairOperationClass)Fair.OCodes[Options[0].Operation.GetType()];

	public Proposal()
	{
	}

	public Proposal(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
	{
		var a = new Proposal(Mcv)
				{	
					Id				= Id,	
					Site			= Site,	
					By				= By,	
					As				= As,
					Flags			= Flags,
					Neither			= Neither,
					Abs				= Abs,
					Ban				= Ban,
					Banish			= Banish,
					CreationTime	= CreationTime,
					Title			= Title,
					Text			= Text,
					Options			= Options,
					Comments		= Comments
				};

		return a;
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<AutoId>();
		Site			= reader.Read<AutoId>();
		By				= reader.Read<AutoId>();
		As				= reader.Read<Role>();
		Flags			= reader.Read<ProposalFlags>();
		Neither			= reader.ReadArray<AutoId>();
		Abs				= reader.ReadArray<AutoId>();
		Ban				= reader.ReadArray<AutoId>();
		Banish			= reader.ReadArray<AutoId>();
		CreationTime	= reader.Read<Time>();
 		Title			= reader.ReadUtf8();
 		Text			= reader.ReadUtf8();
		Options			= reader.ReadArray<ProposalOption>();
		Comments		= reader.ReadArray<AutoId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.Write(By);
		writer.Write(As);
		writer.Write(Flags);
		writer.Write(Neither);
		writer.Write(Abs);
		writer.Write(Ban);
		writer.Write(Banish);
		writer.Write(CreationTime);
 		writer.WriteUtf8(Title);
 		writer.WriteUtf8(Text);
		writer.Write(Options);
		writer.Write(Comments);
	}
}
