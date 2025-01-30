namespace Uccs.Smp;

public enum PublicationChange : byte
{
	None,
	Status,
	Delete,
	Sections,
	Product,
}

public class PublicationUpdation : SmpOperation
{
	public EntityId				Publication { get; set; }
	public PublicationChange	Change { get; set; }
	public object				Value { get; set; }

	public override bool		IsValid(Mcv mcv) => Publication != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}, [{Change}]";

	string[]					Strings  => Value as string[];
	EntityId					EntityId  => Value as EntityId;

	public PublicationUpdation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Change		= reader.ReadEnum<PublicationChange>();
		
		Value = Change switch
					   {
							PublicationChange.Status	=> reader.ReadEnum<PublicationStatus>(),
							PublicationChange.Sections	=> reader.ReadArray(reader.ReadUtf8),
							PublicationChange.Product	=> reader.Read<EntityId>(),
							_ => throw new IntegrityException()
					   };
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case PublicationChange.Status	:	writer.WriteEnum((PublicationStatus)Value); break;
			case PublicationChange.Sections :	writer.Write(Strings, writer.WriteUtf8); break;
			case PublicationChange.Product	:	writer.Write(EntityId); break;
			default:
				throw new IntegrityException();
		}
	}

	public override void Execute(SmpMcv mcv, SmpRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p))
			return;

		p = round.AffectPublication(Publication);

		switch(Change)
		{
			case PublicationChange.Status:
				p.Status = (PublicationStatus)Value;
				break;

			case PublicationChange.Product:
				p.Product = EntityId;
				break;

			case PublicationChange.Sections :
				p.Sections = Strings;
				break;

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