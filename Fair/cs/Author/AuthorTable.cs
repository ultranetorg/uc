using RocksDbSharp;

namespace Uccs.Fair;

public class AuthorTable : Table<AutoId, Author>
{
	public override string	Name => FairTable.Author.ToString();
	public new FairMcv		Mcv => base.Mcv as FairMcv;

	public AuthorTable(FairMcv rds) : base(rds)
	{
	}
		
	public override Author Create()
	{
		return new Author(Mcv);
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var cl in Clusters)
			foreach(var b in cl.Buckets)
				foreach(var i in b.Entries.Where(i => i.Nickname != ""))
				{
					var w = e.Words.Affect(Word.GetId(i.Nickname));

					w.Reference = new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.AuthorNickname};
				}
	
		Mcv.Words.Commit(batch, e.Words.Affected.Values, e.Words, null);
	}
}

public class AuthorExecution : TableExecution<AutoId, Author>
{
	public AuthorExecution(FairExecution execution) : base(execution.Mcv.Authors, execution)
	{
	}

	public Author Create(string name)
	{
		var b = Execution.Mcv.Users.KeyToBucket(name);
		int e = Execution.GetNextEid(Table, b);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(b, e);
		a.Products = [];
		a.Owners = [];
		a.Sites = [];
		a.Links = [];
		a.Files = [];
		a.Nickname = "";			

		Execution.IncrementCount((int)FairMetaEntityType.AuthorsCount);

		return Affected[a.Id] = a;
	}

	public override Author Affect(AutoId id)
	{
		var e = base.Affect(id);

		Execution.TransferEnergyIfNeeded(e);

		return e;
	}

	private void DeleteAuthor(Author author)
	{
	}
}
