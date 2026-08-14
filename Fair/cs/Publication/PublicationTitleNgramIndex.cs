using System.Collections;
using System.Collections.Generic;
using System.Text;
using Roaring.Net.CRoaring;
using RocksDbSharp;

namespace Uccs.Fair;

public class PublicationSearchResult
{
	public Publication	Publication { get; set; }
	public Product		Product { get; set; }
	public Author		Author { get; set; }
	public Category		Category { get; set; }
	
	public int			Distance;

	public override string ToString()
	{
		return $"{Publication.Id}, {nameof(Product)}={Product.Title}, {nameof(Author)}={Author.Title}, {nameof(Category)}={Category.Title}";
	}
}

public class PublicationNgramId : EntityId
{
	public AutoId			Category { get; set; }
	public ulong			Chars { get; set; }
	public override int		Bucket  => (int)(Chars);

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
		return Bucket;
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
	public new FairMcv				Mcv => base.Mcv as FairMcv;
	
	public PublicationTitleNgramIndex(Mcv mcv) : base(mcv, FairTable.PublicationTitle.ToString(), true)
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

	public List<PublicationSearchResult> Search(string query, IEnumerable<AutoId> categories, int skip, int take)
	{
		var r = new SortedSet<PublicationSearchResult>(Comparer<PublicationSearchResult>.Create((a, b) =>	{
																												var r = a.Author.VerifiedWebdomainRank.CompareTo(b.Author.VerifiedWebdomainRank);
								
																												if(r != 0)
																													return r;
								
																												return b.Distance - a.Distance;
																											}));

		foreach(var i in categories)
		{
			var result = Search(query, i, Latest);
	
			var c = Mcv.Categories.Latest(i);

			foreach(var j in result)
			{
				var l = Mcv.Publications.Latest(AutoId.FromULong(j));
				var p = Mcv.Products.Latest(l.Product);
				var a = Mcv.Authors.Latest(p.Author);
			
				r.Add((new PublicationSearchResult
						{
							Publication		= l,
							Author			= a,
							Product			= p,
							Category		= c,
							Distance		= ComputeDistance(query, p.Title)
						}));
			}
		}

		return r.Skip(skip).Take(take).ToList();
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
