namespace Uccs.Fair;

public enum DisputeFlags : byte
{
	Succeeded	= 0b0001,
}

public class Dispute : IBinarySerializable, ITableEntry
{
	public AutoId			Id { get; set; }
	public AutoId			Site { get; set; }
	public DisputeFlags		Flags { get; set; }
	public AutoId[]			Yes { get; set; }
	public AutoId[]			No { get; set; }
	public AutoId[]			Abs { get; set; }
	public Time				Expirtaion { get; set; }
 	public string			Text { get; set; }
	public VotableOperation	Proposal { get; set; }
	public AutoId[]			Comments;

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;
		
	public Dispute()
	{
	}

	public Dispute(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
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
									Comments	= Comments
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
		Id			= reader.Read<AutoId>();
		Site		= reader.Read<AutoId>();
		Flags		= reader.Read<DisputeFlags>();
		Yes			= reader.ReadArray<AutoId>();
		No			= reader.ReadArray<AutoId>();
		Abs			= reader.ReadArray<AutoId>();
		Expirtaion	= reader.Read<Time>();
 		Text		= reader.ReadUtf8();
		//Proposal	= reader.Read<Proposal>();

 		Proposal = GetType().Assembly.GetType(GetType().Namespace + "." + reader.Read<FairOperationClass>()).GetConstructor([]).Invoke(null) as VotableOperation;
 		Proposal.Read(reader); 

		Comments	= reader.ReadArray<AutoId>();
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

		writer.Write(Comments);
	}
}
