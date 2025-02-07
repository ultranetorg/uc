namespace Uccs.Fair;

public enum PublicationChange : byte
{
	None,
	Status,
	ApproveChange,
	RejectChange,
	Delete,
	Product,
}

public class PublicationUpdation : FairOperation
{
	public EntityId				Publication { get; set; }
	public PublicationChange	Change { get; set; }
	public object				Value { get; set; }

	public override bool		IsValid(Mcv mcv) => Publication != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}, [{Change}]";

	string[]					Strings  => Value as string[];
	string						String	 => Value as string;
	EntityId					EntityId => Value as EntityId;

	public PublicationUpdation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Change		= reader.ReadEnum<PublicationChange>();
		
		Value = Change switch
					   {
							PublicationChange.Status		=> reader.ReadEnum<PublicationStatus>(),
							PublicationChange.Product		=> reader.Read<EntityId>(),
							PublicationChange.ApproveChange	=> reader.Read<ProductFieldVersionId>(),
							PublicationChange.RejectChange	=> reader.Read<ProductFieldVersionId>(),
							_ => throw new IntegrityException()
					   };
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case PublicationChange.Status		 : writer.WriteEnum((PublicationStatus)Value); break;
			case PublicationChange.Product		 : writer.Write(EntityId); break;
			case PublicationChange.ApproveChange : writer.Write(Value as ProductFieldVersionId); break;
			case PublicationChange.RejectChange	 : writer.Write(Value as ProductFieldVersionId); break;
			default : throw new IntegrityException();
		}
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p))
			return;

		p = round.AffectPublication(Publication);

		switch(Change)
		{
			case PublicationChange.Status:
			{ 
// 				var s = (ReviewStatus)Value;
// 
// 				if(p.Status == PublicationStatus.Pending && s != ReviewStatus.Pending)
// 				{
// 					var a = round.AffectAuthor(mcv.Products.Find(p.Product, round.Id).AuthorId);
// 
// 					a.ECDeposit
// 
// 					Signer.ECBalanceAdd( r.Reward);
// 					r.Reward = [];
// 				}
// 
				p.Status = (PublicationStatus)Value;
				break;
			}

			case PublicationChange.ApproveChange:
			{
				var v = Value as ProductFieldVersionId;
				var r = round.AffectProduct(p.Product);

				var c = p.Changes.FirstOrDefault(i => i.Name == v.Name && i.Id == v.Id);

				if(c == null)
				{
					Error = NotFound;
					return;
				}
				
				p.Changes = [..p.Changes.Where(i => i != c)];

				var rf = r.Fields.First(i => i.Name == v.Name);
				
				if(rf != null)
				{
					var pf = p.Fields.FirstOrDefault(i => i.Name == v.Name);
	
					if(pf == null)
						p.Fields = [..p.Fields, v];
					else
						p.Fields = [..p.Fields.Where(i => i.Name != v.Name), v];
		
					if(pf != null)
					{
						var x = rf.Versions.First(i => i.Id == pf.Id);
	
						rf = new ProductField {Name = rf.Name, 
											   Versions = [..rf.Versions.Where(i => i.Id != x.Id), new ProductFieldVersion {Id = x.Id, Value = x.Value, Refs = x.Refs - 1}]};
		
						r.Fields = [..r.Fields.Where(i => i.Name != rf.Name), rf];
					}
	
					var y = rf.Versions.First(i => i.Id == v.Id);
	
					rf = new ProductField {Name = rf.Name, 
										  Versions = [..rf.Versions.Where(i => i.Id != y.Id), new ProductFieldVersion {Id = y.Id, Value = y.Value, Refs = y.Refs + 1}]};
	
					r.Fields = [..r.Fields.Where(i => i.Name != v.Name), rf];
				} 
				else
				{
					p.Fields = [..p.Fields.Where(i => i.Name != v.Name)];
				}

				var a = round.AffectAuthor(r.AuthorId);

				var d			 = EC.Take(a.ECDeposit, a.ModerationReward, round.ConsensusTime);
				a.ECDeposit		 = EC.Subtract(a.ECDeposit, a.ModerationReward, round.ConsensusTime);
				Signer.ECBalance = EC.Add(Signer.ECBalance, d);

				break;
			}

			case PublicationChange.RejectChange:
			{	
				var v = Value as ProductFieldVersionId;

				var c = p.Changes.FirstOrDefault(i => i.Name == v.Name && i.Id == v.Id);

				if(c == null)
				{
					Error = NotFound;
					return;
				}
				
				p.Changes = [..p.Changes.Where(i => i != c)];
				break;
			}

			case PublicationChange.Product:
				p.Product = EntityId;
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