using System.Collections;
using System.Collections.Generic;
using System.Text;
using Roaring.Net.CRoaring;
using RocksDbSharp;

namespace Uccs.Fair;

public class ProductSearchResult
{
	public Product		Product { get; set; }
	public Author		Author { get; set; }

	public int			Distance;

	public override string ToString()
	{
		return $"{Product.Id}, Product={Product.Title}, Type={Product.Type}, Author={Author.Title}";
	}
}

public class ProductNgramId : EntityId
{
	public ProductType		Type { get; set; }
	public ulong			Chars { get; set; }
	public override int		Bucket  => (int)(Chars);

	public ProductNgramId()
	{
	}

	public ProductNgramId(ProductType type, ulong b)
	{
		Type = type;
		Chars = b;
	}

	public override string ToString()
	{
		return $"{Type}, {(char)(Chars >> 48)}{(char)(Chars >> 32)}{(char)(Chars >> 16)}{(char)Chars}";
	}

	public override int GetHashCode()
	{
		return Bucket;
	}

	public override void Read(Reader reader)
	{
		Type = reader.Read<ProductType>();
		Chars = (ulong)reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Type);
		writer.Write7BitEncodedInt64((long)Chars);
	}

	public override bool Equals(object obj)
	{
		return obj is ProductNgramId id && Equals(id);
	}

	public override bool Equals(EntityId a)
	{
		return a is ProductNgramId e && Chars == e.Chars && Type == e.Type;
	}

	public override int CompareTo(EntityId a)
	{
		return CompareTo((ProductNgramId)a);
	}

	public int CompareTo(ProductNgramId a)
	{
		var x = Chars.CompareTo(a.Chars);

		if(x != 0)
			return x;

		return Type.CompareTo(a.Type);
	}

	public static bool operator == (ProductNgramId left, ProductNgramId right)
	{
		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (ProductNgramId left, ProductNgramId right)
	{
		return !(left == right);
	}
}

public class ProductTitleNgramIndex : NgramTable<ProductNgramId>
{
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public ProductTitleNgramIndex(Mcv mcv) : base(mcv, FairTable.ProductTitle.ToString(), true)
	{
	}

	public override ProductNgramId CreateId(ulong ngramSpan, object more)
	{
		return new ProductNgramId((ProductType)more, ngramSpan);
	}

	public ProductTitleExecution CreateExecuting(Execution execution)
	{
		return new ProductTitleExecution(execution as FairExecution);
	}

	public List<ProductSearchResult> Search(string query, ProductType type, int skip, int take)
	{
		var r = new SortedSet<ProductSearchResult>(Comparer<ProductSearchResult>.Create((a, b) =>	{
																										var r = a.Author.VerifiedWebdomainRank.CompareTo(b.Author.VerifiedWebdomainRank);
								
																										if(r != 0)
																											return r;
								
																										return a.Distance.CompareTo(b.Distance);
																									}));

		foreach(var t in type == ProductType.None ? Enum.GetValues<ProductType>().Skip(1) : [type])
		{
			var result = base.Search(query, t, Latest);
		
			foreach(var i in result)
			{
				var p = Mcv.Products.Latest(AutoId.FromULong(i));
				var a = Mcv.Authors.Latest(p.Author);
			
				r.Add(	new ProductSearchResult
						{
							Product		= p,
							Author		= a,
							Distance	= ComputeDistance(query, p.Title)
						});
			}
		}

						
		return r.Skip(skip).Take(take).ToList();
	}
}

public class ProductTitleExecution : NgramExecution<ProductNgramId>
{
	public ProductTitleExecution(FairExecution execution) : base(execution, execution.Mcv.ProductTitles)
	{
	}

	//public List<ProductSearchResult> Find(string query, ProductType type)
	//{
	//	var result = base.Search(query, type, 0, 1);
	//	
	//	return Mcv.Products.Latest(AutoId.FromULong(i));
	//}
}
