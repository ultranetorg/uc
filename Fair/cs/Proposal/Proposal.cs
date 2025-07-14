namespace Uccs.Fair;

public enum ProposalFlags : byte
{
}

public class Proposal : IBinarySerializable, ITableEntry
{
	public AutoId			Id { get; set; }
	public AutoId			Site { get; set; }
	public ProposalFlags		Flags { get; set; }
	public AutoId[]			Yes { get; set; }
	public AutoId[]			No { get; set; }
	public AutoId[]			Abs { get; set; }
	public Time				Expirtaion { get; set; }
 	public string			Text { get; set; }
	public VotableOperation	Operation { get; set; }
	public AutoId[]			Comments;

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;
		
	public Proposal()
	{
	}

	public Proposal(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
	{
		var a = new Proposal(Mcv){	
									Id			= Id,	
									Site		= Site,	
									Flags		= Flags,
									Yes			= Yes,
									No			= No,
									Abs			= Abs,
									Expirtaion	= Expirtaion,
									Text		= Text,
									Operation	= Operation,
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
		Flags		= reader.Read<ProposalFlags>();
		Yes			= reader.ReadArray<AutoId>();
		No			= reader.ReadArray<AutoId>();
		Abs			= reader.ReadArray<AutoId>();
		Expirtaion	= reader.Read<Time>();
 		Text		= reader.ReadUtf8();
		//Proposal	= reader.Read<Proposal>();

 		Operation = GetType().Assembly.GetType(GetType().Namespace + "." + reader.Read<FairOperationClass>()).GetConstructor([]).Invoke(null) as VotableOperation;
 		Operation.Read(reader); 

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

		writer.Write(Enum.Parse<FairOperationClass>(Operation.GetType().Name));
		Operation.Write(writer);

		writer.Write(Comments);
	}
}
