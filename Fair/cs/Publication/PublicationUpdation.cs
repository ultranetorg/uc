namespace Uccs.Fair;

public abstract class PublicationUpdation : VotableOperation
{
	protected void Pay(FairRound round, Publication publication, AuthorEntry author)
	{
		var s = round.AffectSite(round.FindCategory(publication.Category).Site);

		author.Energy -= author.ModerationReward;
		Signer.Energy += author.ModerationReward;
			
		EnergyFeePayer = s;
		EnergySpenders.Add(s);
		EnergySpenders.Add(author);
	}
}
