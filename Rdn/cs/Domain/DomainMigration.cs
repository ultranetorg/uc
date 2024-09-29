namespace Uccs.Rdn
{
	public class DomainMigration : RdnOperation
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

		public override void Execute(RdnMcv mcv, RdnRound round)
		{
			var a = mcv.Domains.Find(Name, round.Id);

			if(a?.Owner != null)
			{
				Error = "Already Owned";
				return;
			}

			if(RankCheck)
			{
				if(Signer.GetECBalance(round.ConsensusTime) < (mcv.Net as Rdn).DomainRankCheckECFee)
				{
					Error = NotEnoughEC;
					return;
				}

				Signer.ECBalanceSubtract(round.ConsensusTime, (mcv.Net as Rdn).DomainRankCheckECFee);
				//Transaction.ECReward -= (mcv.Net as RdnZone).DomainRankCheckECFee;
			}
		}

		public void ConfirmedExecute(RdnRound round)
		{
			var a = round.AffectDomain(Name);

			switch(Tld)
			{
				case "com" : a.ComOwner = Signer.Id; break;
				case "org" : a.OrgOwner = Signer.Id; break;
				case "net" : a.NetOwner = Signer.Id; break;
			}

			if((a.ComOwner == Signer.Id && a.OrgOwner == Signer.Id && a.NetOwner == Signer.Id) || RankApproved)
			{
				a.Owner = Signer.Id;
			}
		}
	}
}
