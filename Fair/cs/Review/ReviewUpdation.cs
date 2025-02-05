namespace Uccs.Fair;

public enum ReviewChange : byte
{
	None,
	Status,
	Delete,
	Text,
}

public class ReviewUpdation : FairOperation
{
	public EntityId				Review { get; set; }
	public ReviewChange			Change { get; set; }
	public object				Value { get; set; }

	public override bool		IsValid(Mcv mcv) => Review != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}, [{Change}]";

	string[]					Strings  => Value as string[];
	EntityId					EntityId  => Value as EntityId;

	public ReviewUpdation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Review	= reader.Read<EntityId>();
		Change	= reader.ReadEnum<ReviewChange>();
		
		Value = Change switch
					   {
							ReviewChange.Status	=> reader.ReadEnum<ReviewStatus>(),
							ReviewChange.Text	=> reader.ReadUtf8(),
							_ => throw new IntegrityException()
					   };
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Review);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case ReviewChange.Status	:	writer.WriteEnum((ReviewStatus)Value); break;
			case ReviewChange.Text		:	writer.Write(EntityId); break;
			default:
				throw new IntegrityException();
		}
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireReviewAccess(round, Review, Signer, out var r))
			return;

		r = round.AffectReview(Review);

		switch(Change)
		{
			case ReviewChange.Status:
			{
				var s = (ReviewStatus)Value;
				
				//if(r.Reward.Length > 0 && r.Status == ReviewStatus.Pending && s != ReviewStatus.Pending)
				//{
				//	Signer.ECBalanceAdd(r.Reward);
				//	r.Reward = [];
				//}

				r.Status = s;
				break;
			}
			case ReviewChange.Text:
				r.Text = Value as string;
				break;

			//case ReviewChange.Sections :
			//	p.Sections = Strings;
			//	break;

			///case TopicChange.AddPages:
			///{	
			///	if(!RequirePage(round, EntityId, out var c))
			///		return;
			///
			///	if(c.Parent != null)
			///	{
			///		Error = AlreadyChild;
			///		return;
			///	}
			///
			///	c = round.AffectPage(EntityId);
			///
			///	c.Parent = Page;
			///	p.Fields |= PageField.Parent;
			///	p.Pages = [..p.Pages, EntityId];
			///
			///	break;
			///}
			///
			///case TopicChange.RemovePages:
			///{	
			///	if(RequirePage(round, EntityId, out var c) == false)
			///		return;
			///
			///	c = round.AffectPage(EntityId);
			///
			///	c.Parent = null;
			///	p.Pages = p.Pages.Where(i => i != EntityId).ToArray();
			///	
			///	if(p.Pages.Length == 0)
			///		p.Fields &= ~PageField.Parent;
			///
			///	break;
			///}
			///
			///case TopicChange.Security:
			///{
			///	p.AffectSecurity();
			///
			///	foreach(var i in Security.Permissions)
			///	{
			///		if(i.Value[0] == Actor.None)
			///			p.Security.Permissions.Remove(i.Key);
			///		else
			///			p.Security.Permissions[i.Key] = i.Value;
			///	}
			///	break;
			///}
		}
	}
}