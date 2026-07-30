using System.Collections;
using System.Collections.Generic;
using System.Text;
using Roaring.Net.CRoaring;
using RocksDbSharp;

namespace Uccs.Fair;

public class PublicationNgramId : EntityId
{
	public AutoId			Category { get; set; }
	public ulong			Chars { get; set; }
	public override int		B  => (int)(Chars);

	public PublicationNgramId()
	{
	}

	public PublicationNgramId(AutoId type, ulong b)
	{
		Category = type;
		Chars = b;
	}

	public override string ToString()
	{
		return $"{Category}, {(char)(Chars >> 32)}{(char)(Chars >> 16)}{(char)Chars}";
	}

	public override int GetHashCode()
	{
		return B;
	}

	public override void Read(Reader reader)
	{
		Category = reader.Read<AutoId>();
		Chars = (ulong)reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Category);
		writer.Write7BitEncodedInt64((long)Chars);
	}

	public override bool Equals(object obj)
	{
		return obj is PublicationNgramId id && Equals(id);
	}

	public override bool Equals(EntityId a)
	{
		return a is PublicationNgramId e && Chars == e.Chars && Category == e.Category;
	}

	public override int CompareTo(EntityId a)
	{
		return CompareTo((PublicationNgramId)a);
	}

	public int CompareTo(PublicationNgramId a)
	{
		var x = Chars.CompareTo(a.Chars);

		if(x != 0)
			return x;

		return Category.CompareTo(a.Category);
	}

	public static bool operator == (PublicationNgramId left, PublicationNgramId right)
	{
		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (PublicationNgramId left, PublicationNgramId right)
	{
		return !(left == right);
	}
}

public class PublicationTitleNgramIndex : NgramTable<PublicationNgramId>
{
	public override string			Name => FairTable._PublicationTitle.ToString();
	public new FairMcv				Mcv => base.Mcv as FairMcv;
	
	public PublicationTitleNgramIndex(Mcv mcv) : base(mcv)
	{
	}

	public override PublicationNgramId CreateId(ulong ngramSpan, object more)
	{
		return new PublicationNgramId((AutoId)more, ngramSpan);
	}

	public PublicationTitleExecution CreateExecuting(Execution execution)
	{
		return new PublicationTitleExecution(execution as FairExecution);
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in Mcv.Publications.GraphEntities)
		{
			if(i.IsPublished)
				e.PublicationTitles.Index(i);
		}
		
		Commit(batch, e.PublicationTitles.Affected.Values, null, null);
	}

	public List<PublicationSearchResult> Search(string query, AutoId[] categories, int skip, int take)
	{
		var o = new List<PublicationSearchResult>();

		foreach(var c in categories)
		{
			var result = Search(query, c, Latest, skip, take);
	
			foreach(var i in result)
			{
				var p = Mcv.Publications.Latest(AutoId.FromULong(i));
				var r = Mcv.Products.Latest(p.Product);
				var a = Mcv.Authors.Latest(r.Author);
			
				o.Add(	new PublicationSearchResult
						{
							Publication		= p.Id,
							ProductTitle	= r.Title,
							Author			= a.Id,
							AuthorTitle		= a.Title,
							Logo			= r.Versions.LastOrDefault()?.Fields.FirstOrDefault(i => i.Name == Token.Logo)?.AsAutoId,
							Rank			= a.VerifiedWebdomainRank
						});
			}
		}

		o.Sort((x, y) =>	{
								var r = x.Rank.CompareTo(y.Rank);
								
								if(r != 0)
									return r;
								
								return JaroWinkler.GetSimilarityFixed(query, y.ProductTitle) - JaroWinkler.GetSimilarityFixed(query, x.ProductTitle);
							});

		return o;
	}
}

public class PublicationTitleExecution : NgramExecution<PublicationNgramId>
{
	public PublicationTitleExecution(FairExecution execution) : base(execution, execution.Mcv.PublicationTitles)
	{
	}

	public void Index(Publication publication)
	{
		var p = Execution.Products.Find(publication.Product);

		if(!Execution.ProductTitles.IsIndexed(p.Title, p.Type, p.Id))
		{
			Execution.ProductTitles.Index(p.Title, p.Type, p.Id);
		}
		
		Index(p.Title, publication.Category, publication.Id);
	}

	public void Deindex(Publication publication)
	{
		var p = Execution.Products.Find(publication.Product);

		if(Execution.ProductTitles.IsIndexed(p.Title, p.Type, p.Id))
		{
			Execution.ProductTitles.Deindex(p.Title, p.Type, p.Id);
		}
		
		Deindex(p.Title, publication.Category, publication.Id);
	}
}
