using System.IO;

namespace Uccs.Net
{
	public class AuthorTransfer : Operation
	{
		public string			Author;
		public AccountAddress	To  {get; set;}
		public override string	Description => $"{Author} -> {To}";
		public override bool	Valid => 0 < Author.Length;
		
		public AuthorTransfer()
		{
		}

		public AuthorTransfer(string name, AccountAddress to)
		{
			Author = name;
			To = to;
		}

		protected override void ReadConfirmed(BinaryReader r)
		{
			Author	= r.ReadUtf8();
			To		= r.ReadAccount();
		}

		protected override void WriteConfirmed(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.Write(To);
		}

		public override void Execute(Mcv chain, Round round)
		{
			if(chain.Authors.Find(Author, round.Id).Owner != Signer)
			{
				Error = NotOwner;
				return;
			}

			//round.AffectAccount(Signer).Authors.Remove(Author);
			//round.AffectAccount(To).Authors.Add(Author);

			//round.AffectAuthor(Author).ObtainedRid = round.Id;
			round.AffectAuthor(Author).Owner = To;
		}
	}
}
