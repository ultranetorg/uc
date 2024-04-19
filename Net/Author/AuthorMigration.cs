using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AuthorMigration : Operation//, IEquatable<AuthorBid>
	{
		public string			Author;
		public string			Tld;
		public bool				RankCheck;
		public override string	Description => $"{Author}.{Tld}{(RankCheck ? $", RankCheck" : null)}";
		
		public bool				DnsApproved;
		public bool				RankApproved;

		public override bool Valid
		{
			get
			{
				if(!Net.Author.Valid(Author))
					return false;

				if(!Net.Author.IsExclusive(Author))
					return false;

				if(Tld.Length > 8)
					return false;

				return true;
			}
		} 

		public AuthorMigration()
		{
		}

		public AuthorMigration(string name, string tld, bool checkrank)
		{
			Author = name;
			Tld = tld;
			RankCheck = checkrank;
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Author		= reader.ReadUtf8();
			Tld			= reader.ReadUtf8();
			RankCheck	= reader.ReadBoolean();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.WriteUtf8(Author);
			writer.WriteUtf8(Tld);
			writer.Write(RankCheck);
		}

		public void WriteBaseState(BinaryWriter writer)
		{
			writer.Write(Id);
			writer.Write(Signer);
			writer.WriteUtf8(Author);
			writer.WriteUtf8(Tld);
			writer.Write(RankCheck);
		}

		public void ReadBaseState(BinaryReader reader)
		{
			_Id	= reader.Read<OperationId>();

			Transaction = new ();
			
			Transaction.Signer	= reader.ReadAccount();
			Author				= reader.ReadUtf8();
			Tld					= reader.ReadUtf8();
			RankCheck			= reader.ReadBoolean();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var a = mcv.Authors.Find(Author, round.Id);

			if(a?.Owner != null)
			{
				Error = "Already Owned";
				return;
			}

			if(RankCheck)
			{
				Affect(round, Signer).Balance -= mcv.Zone.DomainRankCheckFee;
			}
		}

		public void ConsensusExecute(Round round)
		{
			var a = Affect(round, Author);

			switch(Tld)
			{
				case "com" : a.ComOwner = Signer; break;
				case "org" : a.OrgOwner = Signer; break;
				case "net" : a.NetOwner = Signer; break;
			}

			if((a.ComOwner == Signer && a.OrgOwner == Signer && a.NetOwner == Signer) || RankApproved)
			{
				a.Owner = Signer;
			}
		}
	}
}
