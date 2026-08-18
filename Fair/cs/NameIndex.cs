namespace Uccs.Fair;

public class NameIndex : TextTable<TextToField<EntityTextField>> 
{
	public NameIndex(Mcv mcv) : base(mcv, FairTable.Name.ToString(), true)
	{
	}
	
	public override TextToField<EntityTextField> Create()
	{
		return new TextToField<EntityTextField>(Mcv);
	}

	public TextToField<EntityTextField> Latest(string text)
	{
		return Latest(new StringId(text));
	}

	public IEnumerable<AutoId> Search(EntityTextField field, string prefix, int count)
	{
		if(prefix.Length < User.NameLengthMin)
			yield break;

		var pre = new StringId(prefix);

		int n = 0;

		var found = new HashSet<AutoId>();

		foreach(var i in (Mcv.LastConfirmedRound as FairRound).Names.Affected)
		{
			if(i.Value.Entity.Field == field && Bytes.EqualityComparer.Equals(i.Key.Bytes, pre.Bytes, pre.Bytes.Length))
			{
				if(found.Add(i.Value.Entity.Id))
				{
					yield return i.Value.Entity.Id;
	
					n++;
	
					if(n == count)
						yield break;
				}
			}
		}
						
		var b = FindBucket(pre.Bucket);

		if(b != null)
		{
			foreach(var i in b.Entries)
			{
				if(i.Entity.Field == field && Bytes.EqualityComparer.Equals(i.Id.Bytes, pre.Bytes, pre.Bytes.Length))
				{
					if(found.Add(i.Entity.Id))
					{
						yield return i.Entity.Id;
	
						n++;
	
						if(n == count)
							yield break;
					}
				}
			}
		}
	}
//		if(pre.Bytes.Length == 2)
//		{
//			var c = FindCluster(ClusterFromBucket(pre.B));
//			
//			if(c != null)
//				foreach(var i in c.Buckets.Where(b => (b.Id >> 8 & 0b11) == (id.B & 0b11)))
//				{
//					foreach(var r in i.Entries.Where(i => i.Reference.Field == field).Select(i => i.Reference))
//					{
//						if(!found.Contains(r.Entity))
//						{
//							found.Add(r.Entity);
//							yield return r.Entity;
//	
//							n++;
//	
//							if(n > count)
//								yield break;
//						}
//					}
//				}
//		}
//
//		/// MAY BE TOO SLOW
//		if(pre.Bytes.Length == 1)
//		{
//			foreach(var c in Clusters.Where(c => (c.Id >> 6) == (pre.B & 0xf)).SelectMany(j => j.Buckets))
//			{
//				foreach(var r in c.Entries.Where(i => i.Reference.Field == field && i.Id.Bytes[0] == pre.Bytes[0]).Select(i => i.Reference))
//				{
//					if(!found.Contains(r.Entity))
//					{
//						found.Add(r.Entity);
//						yield return r.Entity;
//	
//						n++;
//	
//						if(n > count)
//							yield break;
//					}
//				}
//			}
//		}
//	}
}

public class NameExecution : TextExecution<TextToField<EntityTextField>, NameIndex>
{
	public NameExecution(FairExecution execution) : base(execution.Mcv.Names, execution)
	{
	}

	public TextToField<EntityTextField> Find(string text)
	{
		return Find(new StringId(text));
	}

	public TextToField<EntityTextField> Affect(string text)
	{
		return Affect(new StringId(text));
	}

	public override TextToField<EntityTextField> Affect(StringId id)
	{
		if(Affected.TryGetValue(id, out var a))
			return a;
			
		if(Parent != null)
			a = Parent.Find(id);
		else if(!(Execution.Round as FairRound).Names.Affected.TryGetValue(id, out a))
			a = Table.Find(id);

		if(a == null)
		{
			Execution.IncrementMetaInt(FairMetaEntityType.NamesCount);

			a = Table.Create();
			a.Id = id;
		
			return Affected[id] = a;
		} 
		else
		{
			return Affected[id] = a.Clone() as TextToField<EntityTextField>;
		}
	}

	public void Register(string word, EntityTextField field, AutoId entity)
	{
		var w = Affect(new StringId(word));
	
		w.Entity = new EntityField<EntityTextField> {Id = entity, Field = field};
	}

	public void Unregister(string word)
	{
		var w = Affect(new StringId(word));

		w.Deleted = true;
	}
}