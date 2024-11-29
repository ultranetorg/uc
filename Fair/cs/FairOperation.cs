namespace Uccs.Fair
{
	public enum FairOperationClass
	{
		None = 0, 
		FairCandidacyDeclaration	= OperationClass.CandidacyDeclaration, 
		//Immission				= OperationClass.Immission, 
		UtilityTransfer			= OperationClass.UtilityTransfer, 
		BandwidthAllocation		= OperationClass.BandwidthAllocation,

		ChildNetInitialization,

		PublisherRegistration, PublisherMigration, PublisherBid, PublisherUpdation,
		ProductCreation, ProductUpdation, ProductDeletion, ProductLinkCreation, ProductLinkDeletion,
		AnalysisResultUpdation
	}

	public abstract class FairOperation : Operation
	{
		public const string		CantChangeSealedProduct = "Cant change sealed resource";
		public const string		NotRoot = "Not root domain";

		public abstract void Execute(FairMcv mcv, FairRound round);

		public override void Execute(Mcv mcv, Round round)
		{
			Execute(mcv as FairMcv, round as FairRound);
		}

		public void PayForSpacetime(int length, Time time)
		{
			var fee = SpacetimeFee(length, time);
			
			Signer.BYBalance	 -= fee;
			Transaction.BYReward += fee;
		}

		public static long SpacetimeFee(int length, Time time)
		{
			return Mcv.ApplyTimeFactor(time, length);
		}

		public void Allocate(Round round, Publisher domain, int toallocate)
		{
			if(domain.SpaceReserved < domain.SpaceUsed + toallocate)
			{
				var f = SpacetimeFee(domain.SpaceUsed + toallocate - domain.SpaceReserved, domain.Expiration - round.ConsensusTime);

				Signer.BYBalance	 -= f;
				Transaction.BYReward += f;
	
				domain.SpaceReserved = 
				domain.SpaceUsed = (short)(domain.SpaceUsed + toallocate);
			}
			else
				domain.SpaceUsed += (short)toallocate;
		}

		public void Free(Publisher domain, int tofree) /// WE DONT REFUND
		{
			//var f = SpacetimeFee(tofree, domain.Expiration - round.ConsensusTime);

			domain.SpaceUsed -= (short)tofree;
		
			//Signer.STBalance += f;
			//STReward -= f;
		}

		public bool RequireSignerPublisher(FairRound round, EntityId id, out PublisherEntry domain)
		{
			domain = round.Mcv.Publishers.Find(id, round.Id);

			if(domain == null)
			{
				Error = NotFound;
				return false;
			}

			if(Publisher.IsExpired(domain, round.ConsensusTime))
			{
				Error = Expired;
				return false;
			}

			if(domain.Owner != Signer.Id)
			{
				Error = NotOwner;
				return false;
			}

			return true;
		}

		public bool RequirePublisher(FairRound round, EntityId id, out PublisherEntry domain)
		{
			domain = round.Mcv.Publishers.Find(id, round.Id);

			if(domain == null)
			{
				Error = NotFound;
				return false;
			}

			if(Publisher.IsExpired(domain, round.ConsensusTime))
			{
				Error = Expired;
				return false;
			}

			return true;
		}

		public bool RequireProduct(FairRound round, ProductId id, out PublisherEntry publisher, out Product product)
		{
			product = null;

			if(RequirePublisher(round, id.PublisherId, out publisher) == false)
			{
				Error = NotFound;
				return false; 
			}

			var a = round.Mcv.Assortments.Find(publisher.Id, round.Id);

			product = a.Products.FirstOrDefault(i => i.Id == id);
			
			if(product == null)
			{
				Error = NotFound;
				return false; 
			}

			return true; 
		}

		public bool RequireSignerProduct(FairRound round, ProductId id, out PublisherEntry publisher, out Product product)
		{
			product = null;

			if(RequireSignerPublisher(round, id.PublisherId, out publisher) == false)
			{
				Error = NotFound;
				return false; 
			}

			var a = round.Mcv.Assortments.Find(publisher.Id, round.Id);

			product = a.Products.FirstOrDefault(i => i.Id == id);
			
			if(product == null)
			{
				Error = NotFound;
				return false; 
			}

			return true; 
		}
	}
}
