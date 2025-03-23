namespace Uccs.Fair;

public enum DisputeFlags : byte
{
	Succeeded	= 0b0001,
}

public class Dispute : IBinarySerializable, ITableEntry
{
	public EntityId					Id { get; set; }
	public EntityId					Site { get; set; }
	public DisputeFlags				Flags { get; set; }
	public EntityId[]				Yes { get; set; }
	public EntityId[]				No { get; set; }
	public EntityId[]				Abs { get; set; }
	public Time						Expirtaion { get; set; }
 	public string					Text { get; set; }
	public VotableOperation	Proposal { get; set; }

	public BaseId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;
		
	public Dispute()
	{
	}

	public Dispute(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public Dispute Clone()
	{
		var a = new Dispute(Mcv){	
										Id			= Id,	
										Site		= Site,	
										Flags		= Flags,
										Yes			= Yes,
										No			= No,
										Abs			= Abs,
										Expirtaion	= Expirtaion,
										Text		= Text,
										Proposal	= Proposal,
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
		Id			= reader.Read<EntityId>();
		Site		= reader.Read<EntityId>();
		Flags		= reader.Read<DisputeFlags>();
		Yes			= reader.ReadArray<EntityId>();
		No			= reader.ReadArray<EntityId>();
		Abs			= reader.ReadArray<EntityId>();
		Expirtaion	= reader.Read<Time>();
 		Text		= reader.ReadUtf8();
		//Proposal	= reader.Read<Proposal>();

 		Proposal = GetType().Assembly.GetType(GetType().Namespace + "." + reader.Read<FairOperationClass>()).GetConstructor([]).Invoke(null) as VotableOperation;
 		Proposal.Read(reader); 
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.Write(Flags);
		writer.Write(Yes);
		writer.Write(No);
		writer.Write(Abs);
		writer.Write(Expirtaion);
 		writer.WriteUtf8(Text);
		//writer.Write(Proposal);

		writer.Write(Enum.Parse<FairOperationClass>(Proposal.GetType().Name));
		Proposal.Write(writer);
	}
}
