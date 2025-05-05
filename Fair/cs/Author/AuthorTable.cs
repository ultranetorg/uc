using RocksDbSharp;

namespace Uccs.Fair;

public class AuthorTable : Table<AutoId, Author>
{
	public new FairMcv	Mcv => base.Mcv as FairMcv;

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

					w.References = [..w.References, new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.AuthorNickname}];
				}
	
		Mcv.Words.Commit(batch, e.Words.Affected.Values, null, null);
	}
}

public class AuthorExecution : TableExecution<AutoId, Author>
{
	public AuthorExecution(FairExecution execution) : base(execution.Mcv.Authors, execution)
	{
	}

	public Author Create(AccountAddress signer)
	{
		var b = Execution.Mcv.Accounts.KeyToBucket(signer);
		
		int e = Execution.GetNextEid(Table, b);

		var a = Table.Create();
		a.Id = new AutoId(b, e);
		a.Products = [];
		a.Owners = [];
		a.Sites = [];
		a.Nickname = "";			

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
