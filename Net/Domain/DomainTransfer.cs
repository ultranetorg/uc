using System.IO;

namespace Uccs.Net
{
	public class DomainTransfer : Operation
	{
		public string			Name;
		public AccountAddress	To  {get; set;}
		public override string	Description => $"{Name} -> {To}";
		public override bool	Valid => 0 < Name.Length;
		
		public DomainTransfer()
		{
		}

		public DomainTransfer(string name, AccountAddress to)
		{
			Name = name;
			To = to;
		}

		public override void ReadConfirmed(BinaryReader r)
		{
			Name	= r.ReadUtf8();
			To		= r.ReadAccount();
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			w.WriteUtf8(Name);
			w.Write(To);
		}

		public override void Execute(Mcv chain, Round round)
		{
			if(chain.Domains.Find(Name, round.Id).Owner != Signer)
			{
				Error = NotOwner;
				return;
			}

			Affect(round, Name).Owner = To;
		}
	}
}
