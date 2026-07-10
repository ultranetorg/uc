using System.Collections;
using System.Collections.Generic;
using System.Text;
using RocksDbSharp;

namespace Uccs.Fair;

public class PublicationHnswEntity : IBinarySerializable
{
	public AutoId		Store { get; set; }
	public AutoId		Id { get; set; }

	public void Read(Reader reader)
	{
		Store = reader.Read<AutoId>();
		Id   = reader.Read<AutoId>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Store);
		writer.Write(Id);
	}
}

public class ProductHnswEntity : IBinarySerializable
{
	public AutoId						Id { get; set; }
	public PublicationHnswEntity[]		Publications { get; set; }

	public void Read(Reader reader)
	{
		Id = reader.Read<AutoId>();
		Publications = reader.ReadArray<PublicationHnswEntity>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		writer.Write(Publications);
	}
}

public class ProductTitleHnswEntity : StringHnswEntity
{
	public ProductHnswEntity[]		Products { get; set; }

	public override object Clone()
	{
		var a = new ProductTitleHnswEntity {Products = Products};
		
		Copy(a);

		return a;
	}

	public override string ToString()
	{
		return $"{base.ToString()}, Products={{{Products.Length}}}";
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
		Products = reader.ReadArray<ProductHnswEntity>();
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);
		writer.Write(Products);
	}
}

public class ProductTitleIndex : HnswTable<string, ProductTitleHnswEntity>
{
	public override string			Name => FairTable._ProductTitle.ToString();

	public ProductTitleIndex(Mcv mcv) : base(mcv, new NeedlemanWunsch())
	{
	}

	public override ProductTitleHnswEntity Create()
	{
		return new ProductTitleHnswEntity {Products = [], Text = string.Empty};
	}

	public ProductTitleExecution CreateExecuting(Execution execution)
	{
		return new ProductTitleExecution(execution as FairExecution);
	}


	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in Mcv.Publications.GraphEntities)
		{
			if(i.IsPublished)
				e.ProductTitles.Index(i);
		}
			
		Commit(batch, e.ProductTitles.Affected.Values, null, null);
	}

	public List<ProductSearchResult> Search(AutoId store, string query, int skip, int take)
	{
		var result = Search(query.ToLowerInvariant(), 
							skip, 
							take, 
							i => i.Products.Any(p => p.Publications.Any(u => u.Store == store)),
							Mcv.ProductTitles.Latest);

		var r = new List<ProductSearchResult>();

		foreach(var i in result)
		{
			foreach(var p in i.Products)
			{
				var b = p.Publications.FirstOrDefault(i => i.Store == store);
				
				if(b != null)
				{
					var pub = Mcv.Publications.Latest(b.Id);
					var prod = Mcv.Products.Latest(p.Id);
					var auth = Mcv.Authors.Latest(prod.Author);
	
					r.Add(	new ProductSearchResult
							{
								Product			= prod.Id,
								ProductTitle	= prod.Versions.First(i => i.Id == pub.ProductVersion).Fields.First(i => i.Name == Token.Title).AsUtf8,
								Author			= auth.Id,
								AuthorTitle		= auth.Title,
								Avatar			= prod.Versions.First(i => i.Id == pub.ProductVersion).Fields.FirstOrDefault(i => i.Name == Token.Logo)?.AsAutoId,
								Publications	= prod.Publications,
								Rank			= auth.VerifiedWebdomainRank
							});
				}
			}
		}

		r.Sort((x, y) => x.Rank.CompareTo(y.Rank));

		return r;
	}

	public List<ProductSearchResult> Search(string query, int skip, int take)
	{
		var result = Search(query.ToLowerInvariant(), 
							skip, 
							take, 
							null,
							Mcv.ProductTitles.Latest);

		var r = new List<ProductSearchResult>();

		foreach(var t in result)
		{
			foreach(var p in t.Products)
			{
				var prod = Mcv.Products.Latest(p.Id);
				var auth = Mcv.Authors.Latest(prod.Author);
				var pub = Mcv.Publications.Latest(prod.Publications[0]);
	
				r.Add(	new ProductSearchResult
						{
							Product			= prod.Id,
							ProductTitle	= prod.Versions.First(i => i.Id == pub.ProductVersion).Fields.First(i => i.Name == Token.Title).AsUtf8,
							Author			= auth.Id,
							AuthorTitle		= auth.Title,
							Avatar			= prod.Versions.First(i => i.Id == pub.ProductVersion).Fields.FirstOrDefault(i => i.Name == Token.Logo)?.AsAutoId,
							Publications	= prod.Publications,
							Rank			= auth.VerifiedWebdomainRank
						});
			}
		}

		r.Sort((x, y) => x.Rank.CompareTo(y.Rank));

		return r;
	}

}

public class ProductTitleExecution : StringHnswTableExecution<ProductTitleHnswEntity>
{
	public ProductTitleExecution(FairExecution execution) : base(execution, execution.Mcv.ProductTitles)
	{
	}

  	public void Index(Publication publication)
  	{
 		var e = Index(Execution.Products.Find(publication.Product).Versions.First(i => i.Id == publication.ProductVersion).Fields.First(i => i.Name == Token.Title).AsUtf8);
 
		var p = e.Products.FirstOrDefault(i => i.Id == publication.Product);
		
		if(p == null)
		{
			p = new ProductHnswEntity {Id = publication.Product, Publications = []};
			e.Products = [..e.Products, p];
		}

		var b = p.Publications.FirstOrDefault(i => i.Id == publication.Id);
		
		if(b == null) 
		{
			b = new PublicationHnswEntity {Id = publication.Id, Store = publication.Store};
			p.Publications = [..p.Publications, b];
		}
  	}
 
  	public void Deindex(Publication publication)
  	{
 		var e = Affect(Execution.Products.Find(publication.Product).Versions.First(i => i.Id == publication.ProductVersion).Fields.First(i => i.Name == Token.Title).AsUtf8);

		var p = e.Products.FirstOrDefault(i => i.Id == publication.Product);
		
		if(p != null)
		{
			var b = p.Publications.FirstOrDefault(i => i.Id == publication.Id);
	
			if(p != null)
			{
				if(p.Publications.Length > 1)
				{
					e.Products = e.Products.Replace(p, new (){Id = p.Id, Publications = p.Publications.Remove(b)});	
				} 
				else
				{
					e.Products = e.Products.Remove(p);	
				}
			}
		}
  	}
}
