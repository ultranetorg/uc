namespace Uccs.Fair;

public class StoreModeratorRemoval : StoreOperation
{
	public AutoId				Moderator { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Store={Store}, Moderator={Moderator}";
	
	public override void Read(Reader reader)
	{
		Moderator = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Moderator);
	}

	public override void Execute(FairExecution execution)
	{
 		var s = Store;
 
		s.Moderators = s.Moderators.Remove(s.Moderators.First(m => m.User == Moderator));
	}
}