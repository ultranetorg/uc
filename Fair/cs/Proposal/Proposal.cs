namespace Uccs.Fair;

public enum ProposalFlags : byte
{
}

public class Proposal : IBinarySerializable, ITableEntry
{
	public AutoId			Id { get; set; }
	public AutoId			Site { get; set; }
	public AutoId			Creator { get; set; }
	public Role				As { get; set; }
	public ProposalFlags	Flags { get; set; }
	public AutoId[]			Yes { get; set; }
	public AutoId[]			No { get; set; }
	public AutoId[]			NoAndBan { get; set; }
	public AutoId[]			NoAndBanish { get; set; }
	public AutoId[]			Abs { get; set; }
	public Time				Expiration { get; set; }
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
		var a = new Proposal(Mcv)
				{	
					Id			= Id,	
					Site		= Site,	
					Creator		= Creator,	
					As = As,
					Flags		= Flags,
					Yes			= Yes,
					No			= No,
					NoAndBan	= NoAndBan,
					NoAndBanish	= NoAndBanish,
					Abs			= Abs,
					Expiration	= Expiration,
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
		Creator		= reader.Read<AutoId>();
		As	= reader.Read<Role>();
		Flags		= reader.Read<ProposalFlags>();
		Yes			= reader.ReadArray<AutoId>();
		No			= reader.ReadArray<AutoId>();
		NoAndBan	= reader.ReadArray<AutoId>();
		NoAndBanish	= reader.ReadArray<AutoId>();
		Abs			= reader.ReadArray<AutoId>();
		Expiration	= reader.Read<Time>();
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
		writer.Write(Creator);
		writer.Write(As);
		writer.Write(Flags);
		writer.Write(Yes);
		writer.Write(No);
		writer.Write(NoAndBan);
		writer.Write(NoAndBanish);
		writer.Write(Abs);
		writer.Write(Expiration);
 		writer.WriteUtf8(Text);
		//writer.Write(Proposal);

		writer.Write(Enum.Parse<FairOperationClass>(Operation.GetType().Name));
		Operation.Write(writer);

		writer.Write(Comments);
	}
}
