using System.Collections;
using System.Collections.Generic;
using System.Text;
using Roaring.Net.CRoaring;
using RocksDbSharp;

namespace Uccs.Fair;

public class StoreSearchResult
{
	public Store		Store { get; set; }
	public int			Distance;

	public override string ToString()
	{
		return $"{Store.Title}";
	}
}

public class StoreNgramId : EntityId
{
	public ulong			Chars { get; set; }
	public override int		Bucket  => (int)(Chars);

	public StoreNgramId()
	{
	}

	public StoreNgramId(ulong b)
	{
		Chars = b;
	}

	public override string ToString()
	{
		return $"{(char)Chars >> 48}{(char)Chars >> 32}{(char)Chars >> 16}{(char)Chars}";
	}

	public override int GetHashCode()
	{
		return Bucket;
	}

	public override void Read(Reader reader)
	{
		Chars = (ulong)reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt64((long)Chars);
	}

	public override bool Equals(object obj)
	{
		return obj is StoreNgramId id && Equals(id);
	}

	public override bool Equals(EntityId a)
	{
		return a is StoreNgramId e && Chars == e.Chars;
	}

	public override int CompareTo(EntityId a)
	{
		return CompareTo((StoreNgramId)a);
	}

	public int CompareTo(StoreNgramId a)
	{
		return Chars.CompareTo(a.Chars);
	}

	public static bool operator == (StoreNgramId left, StoreNgramId right)
	{
		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (StoreNgramId left, StoreNgramId right)
	{
		return !(left == right);
	}
}

public class StoreTitleNgramIndex : NgramTable<StoreNgramId>
{
	public new FairMcv				Mcv => base.Mcv as FairMcv;
	
	public StoreTitleNgramIndex(Mcv mcv) : base(mcv, FairTable.StoreTitle.ToString(), true)
	{
	}

	public override StoreNgramId CreateId(ulong ngramSpan, object more)
	{
		return new StoreNgramId(ngramSpan);
	}

	public StoreTitleNgramExecution CreateExecuting(Execution execution)
	{
		return new StoreTitleNgramExecution(execution as FairExecution);
	}

	public List<StoreSearchResult> Search(string query, int skip, int take)
	{
		var r = new SortedSet<StoreSearchResult>(Comparer<StoreSearchResult>.Create((a, b) =>	{
																									return a.Distance.CompareTo(b.Distance);
																								}));

		var result = base.Search(query, query, Latest);
		
		foreach(var i in result)
		{
			var s = Mcv.Stores.Latest(AutoId.FromULong(i));
			
			r.Add(	new StoreSearchResult
					{
						Store = s, 
						Distance = ComputeDistance(query, s.Title)
					});
		}

		return r.Skip(skip).Take(take).ToList();
	}
}

public class StoreTitleNgramExecution : NgramExecution<StoreNgramId>
{
	public StoreTitleNgramExecution(FairExecution execution) : base(execution, execution.Mcv.StoreTitles)
	{
	}
}
