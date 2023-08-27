using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Numerics;

namespace Uccs.Net
{
	public class ResourceUpdation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public ResourceChanges		Changes	{ get; set; }
		public byte					Years { get; set; }
		public ResourceFlags		Flags { get; set; }
		public ResourceType			Type { get; set; }
		public byte[]				Data { get; set; }
		public string				Parent { get; set; }
		public Coin					AnalysisFee { get; set; }

		public override bool		Valid=>	(Flags & ResourceFlags.Unchangables) == 0
											&& (!Changes.HasFlag(ResourceChanges.Years) || Changes.HasFlag(ResourceChanges.Years) && Mcv.EntityAllocationYearsMin <= Years && Years <= Mcv.EntityAllocationYearsMax)
											&& (!Changes.HasFlag(ResourceChanges.Data) || Changes.HasFlag(ResourceChanges.Data) && Data.Length <= Net.Resource.DataLengthMax)
											&& (!Changes.HasFlag(ResourceChanges.AnalysisFee) || Changes.HasFlag(ResourceChanges.AnalysisFee) && AnalysisFee > 0)
											;
		public override string		Description => $"{Resource}, [{Changes}], [{Flags}], Years={Years}, {Type}, {(Parent == null ? null : ", Parent=" + Parent)}{(Data == null ? null : ", Data=" + Hex.ToHexString(Data))}{(AnalysisFee == Coin.Zero ? null : ", AnalysisFee=" + AnalysisFee.ToHumanString())}";

		public ResourceUpdation()
		{
		}

		public ResourceUpdation(AccountAddress signer, ResourceAddress resource)
		{
			Signer = signer;
			Resource = resource;
		}
		
		public void Change(byte years)
		{
			Years = years;
			Changes |= ResourceChanges.Years;
		}
		
		public void Change(ResourceFlags flags)
		{
			Flags = flags;
			Changes |= ResourceChanges.Flags;
		}
		
		public void Change(ResourceType type)
		{
			Type = type;
			Changes |= ResourceChanges.Type;
		}
		public void Change(byte[] data)
		{
			Data = data;
			Changes |= ResourceChanges.Data;
		}

		public void Change(string parent)
		{
			Parent = parent;
			Changes |= ResourceChanges.Parent;
		}

		public void Change(Coin analysisfee)
		{
			AnalysisFee = analysisfee;
			Changes |= ResourceChanges.AnalysisFee;
		}

		public void ChangeRecursive()
		{
			Changes |= ResourceChanges.Recursive;
		}

		protected override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Changes = (ResourceChanges)reader.ReadByte();
			
			if(Changes.HasFlag(ResourceChanges.Years))	Years = reader.ReadByte();
			if(Changes.HasFlag(ResourceChanges.Flags))			Flags = (ResourceFlags)reader.ReadByte();
			if(Changes.HasFlag(ResourceChanges.Data))			Data = reader.ReadBytes();
			if(Changes.HasFlag(ResourceChanges.Parent))			Parent = reader.ReadUtf8();
			if(Changes.HasFlag(ResourceChanges.AnalysisFee))	AnalysisFee = reader.ReadCoin();
		}

		protected override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Changes);

			if(Changes.HasFlag(ResourceChanges.Years))	writer.Write(Years);
			if(Changes.HasFlag(ResourceChanges.Flags))			writer.Write((byte)Flags);
			if(Changes.HasFlag(ResourceChanges.Data))			writer.WriteBytes(Data);
			if(Changes.HasFlag(ResourceChanges.Parent))			writer.WriteUtf8(Parent);
			if(Changes.HasFlag(ResourceChanges.AnalysisFee))	writer.Write(AnalysisFee);
		}

		public override void Execute(Mcv chain, Round round)
		{
 			var aa = chain.Authors.Find(Resource.Author, round.Id); /// TODO: Allow to prolongate for non-owner
 
 			if(aa.Owner != Signer)
 			{
 				Error = NotOwner;
 				return;
 			}

			var e = chain.Authors.FindResource(Resource, round.Id);
			
			if(e == null) 
			{
				Error = NotFound;
				return;
			}

			if(Changes.HasFlag(ResourceChanges.Data) && e.Flags.HasFlag(ResourceFlags.Sealed))
			{
				Error = CantChangeSealedResource;
				return;
			}

			var a = round.AffectAuthor(Resource.Author);
			
			void execute(ResourceAddress resource, bool ignore_renewal_errors)
			{
				var r = a.AffectResource(resource);
	
				if(Changes.HasFlag(ResourceChanges.Years))
				{
					if(!ignore_renewal_errors)
					{
						if(round.ConfirmedTime < r.RenewalBegin)
						{
							Error = "Renewal is allowed during last year only";
							return;
						}

						if(round.ConfirmedTime > r.Expiration)
						{
							Error = "Expired";
							return;
						}
					}
					
					r.Expiration += ChainTime.FromYears(Years);
					r.LastRenewalYears = Years;

					PayForAllocation(e.Data == null ? 0 : e.Data.Length, Years);
				}
	
				if(Changes.HasFlag(ResourceChanges.Flags))
				{
					r.Flags = r.Flags & ResourceFlags.Unchangables | Flags & ~ResourceFlags.Unchangables;
				}
	
				if(Changes.HasFlag(ResourceChanges.Type))
				{
					r.Type = Type;
				}
	
				if(Changes.HasFlag(ResourceChanges.Parent))
				{
					if(e.Flags.HasFlag(ResourceFlags.Child)) /// remove from existing parent
					{
						var p = a.Resources.First(i => i.Resources.Contains(e.Id));
						p = a.AffectResource(p.Address);
						p.Resources = p.Resources.Where(i => i != e.Id).ToArray();
					} 
									
					if(Parent == null)
					{
						r.Flags &= ~ResourceFlags.Child;
					}
					else
					{
						r.Flags |= ResourceFlags.Child;
						var p = a.AffectResource(new ResourceAddress(a.Name, Parent));
						p.Resources = p.Resources.Append(r.Id).ToArray();
					}
				}
	
				if(Changes.HasFlag(ResourceChanges.Data))
				{
					if(Data != null && Data.Length > 0)
					{
						r.Flags |= ResourceFlags.Data;
						r.Data = Data;
	
						var d = Data.Length - r.Reserved;
		
						if(d > 0)
						{
							//long s = CalculateSize();
							//r.Expiration = new ChainTime(r.Expiration.Ticks - ChainTime.TicksFromYears(r.LastRenewalYears) + ChainTime.TicksFromYears(r.LastRenewalYears) * s / (s + d));
							
							r.Reserved += (short)d;
		
							round.AffectAccount(Signer).Balance -= Mcv.CalculateSpaceFee(d, r.LastRenewalYears);
						}
					} 
					else
						r.Flags &= ~ResourceFlags.Data;
				}
	
				if(Changes.HasFlag(ResourceChanges.AnalysisFee))
				{
					if(AnalysisFee > 0 && r.AnalysisStage == AnalysisStage.NotRequested)
					{
						round.AffectAccount(Signer).Balance -= AnalysisFee;
		
						r.AnalysisStage = AnalysisStage.Pending;
						r.AnalysisFee = AnalysisFee;
						r.RoundId = round.Id;
						r.AnalysisHalfVotingRound = 0;
					}
				}

				if(Changes.HasFlag(ResourceChanges.Recursive))
				{
					foreach(var i in r.Resources)
					{
						execute(a.Resources[i].Address, true);
					}
				} 
			}

			execute(Resource, false);
		}
	}
}