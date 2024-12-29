namespace Uccs.Fair;

public class TopicDeletion : FairOperation
{
	public EntityId				Topic { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public TopicDeletion()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Topic = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Topic);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireTopicAccess(round, Topic, out var cat, out var c) == false)
			return;

		round.AffectTopic(Topic).Deleted = true;
		
		cat = round.AffectCatalogue(cat.Id);
		cat.Topics = cat.Topics.Where(i => i != Topic).ToArray();

		//Free(d, r.Length);
	}
}