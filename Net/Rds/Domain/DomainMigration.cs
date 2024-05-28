using System.IO;

namespace Uccs.Net
{
	public class DomainMigration : RdsOperation
	{
		public string			Name;
		public string			Tld;
		public bool				RankCheck;
		public override string	Description => $"{Name}.{Tld}{(RankCheck ? $", RankCheck" : null)}";
		
		public bool				DnsApproved;
		public bool				RankApproved;
		public EntityId			Generator;

		public DomainMigration()
		{
		}

		public DomainMigration(string name, string tld, bool checkrank)
		{
			Name = name;
			Tld = tld;
			RankCheck = checkrank;
		}

		public override bool IsValid(Mcv mcv)
		{
			if(!Domain.Valid(Name))
				return false;

			if(!Domain.IsWeb(Name))
				return false;

			if(Tld.Length > 8)
				return false;

			return true;
		} 
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Name		= reader.ReadUtf8();
			Tld			= reader.ReadUtf8();
			RankCheck	= reader.ReadBoolean();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.WriteUtf8(Name);
			writer.WriteUtf8(Tld);
			writer.Write(RankCheck);
		}

		public void WriteBaseState(BinaryWriter writer)
		{
			writer.Write(Id);
			writer.Write(Signer);
			writer.WriteUtf8(Name);
			writer.WriteUtf8(Tld);
			writer.Write(RankCheck);
			writer.Write(Generator);
		}

		public void ReadBaseState(BinaryReader reader)
		{
			_Id	= reader.Read<OperationId>();

			Transaction = new ();
			
			Transaction.Signer	= reader.ReadAccount();
			Name				= reader.ReadUtf8();
			Tld					= reader.ReadUtf8();
			RankCheck			= reader.ReadBoolean();
			Generator			= reader.Read<EntityId>();
		}

		public override void Execute(Rds mcv, RdsRound round)
		{
			var a = mcv.Domains.Find(Name, round.Id);

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

		public void ConfirmedExecute(RdsRound round)
		{
			var a = round.AffectDomain(Name);

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
