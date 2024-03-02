using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceUpdation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public ResourceChanges		Changes	{ get; set; }
		public ResourceFlags		Flags { get; set; }
		public ResourceData			Data { get; set; }
		public string				Parent { get; set; }
		public Money				AnalysisPayment { get; set; }

		public override bool		Valid =>	(!Flags.HasFlag(ResourceFlags.Data) || (Data == null || Data.Value.Length <= ResourceData.LengthMax));
		public override string		Description => $"{Resource}, [{Changes}], [{Flags}], {(Parent == null ? null : ", Parent=" + Parent)}{(Data == null ? null : $", Data={{{Data}}}")}";

		public ResourceUpdation()
		{
		}

		public ResourceUpdation(ResourceAddress resource)
		{
			Resource = resource;
		}
		
		public void Change(ResourceFlags flags)
		{
			Flags = flags;
		}

		public void Change(ResourceData data)
		{
			Data = data;
			Flags |= ResourceFlags.Data;
			
			if(data != null)
				Changes = ResourceChanges.NotNullData;
		}

		public void Change(string parent)
		{
			Parent = parent;
			Flags |= ResourceFlags.Child;
		}

		public void Change(Money analysispayment)
		{
			AnalysisPayment = analysispayment;
			Flags |= ResourceFlags.Analysis;
		}

		public void ChangeRecursive()
		{
			Changes |= ResourceChanges.Recursive;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceAddress>();
			Flags		= (ResourceFlags)reader.ReadByte();
			Changes		= (ResourceChanges)reader.ReadByte();
			
			if(	Flags.HasFlag(ResourceFlags.Data) && 
				Changes.HasFlag(ResourceChanges.NotNullData))	Data = reader.Read<ResourceData>();
			if(Flags.HasFlag(ResourceFlags.Child))				Parent = reader.ReadUtf8();
			if(Flags.HasFlag(ResourceFlags.Analysis))			AnalysisPayment = reader.Read<Money>();

		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Flags);
			writer.Write((byte)Changes);

			if(	Flags.HasFlag(ResourceFlags.Data) && 
				Changes.HasFlag(ResourceChanges.NotNullData))	writer.Write(Data);
			if(Flags.HasFlag(ResourceFlags.Child))				writer.WriteUtf8(Parent);
			if(Flags.HasFlag(ResourceFlags.Analysis))			writer.Write(AnalysisPayment);
		}

		public override void Execute(Mcv mcv, Round round)
		{
 			var ae = mcv.Authors.Find(Resource.Author, round.Id); /// TODO: Allow to prolongate for non-owner
 
 			if(ae.Owner != Signer)
 			{
 				Error = NotOwner;
 				return;
 			}

			if(Author.IsExpired(ae, round.ConsensusTime))
			{
				Error = Expired;
				return;
			}

			var e = ae.Resources.FirstOrDefault(i => i.Address == Resource);
			
			if(e == null) 
			{
				Error = NotFound;
				return;
			}

			var a = Affect(round, Resource.Author);
			
			void execute(ResourceAddress resource)
			{
				Fee += round.ConsensusExeunitFee;

				var r = a.AffectResource(resource.Resource);
	
				r.Flags	|= Flags&ResourceFlags.Sealed;

				if(Flags.HasFlag(ResourceFlags.Child))
				{
					if(e.Flags.HasFlag(ResourceFlags.Child)) /// remove from existing parent
					{
						var p = a.Resources.First(i => i.Resources.Contains(e.Id.Ri));
						p = a.AffectResource(p.Address.Resource);
						p.Resources = p.Resources.Where(i => i != e.Id.Ri).ToArray();
					} 

					if(Parent != null)
					{
						r.Flags	|= ResourceFlags.Child;
						var i = a.Resources.FirstOrDefault(i => i.Address.Resource == Parent);

						if(i == null)
						{
							Error = NotFound;
							return;
						}

						var p = a.AffectResource(Parent);
						p.Resources = p.Resources.Append(r.Id.Ri).ToArray();
					}
					else
						r.Flags	&= ~ResourceFlags.Child;
				}
	
				if(Flags.HasFlag(ResourceFlags.Data))
				{
					if(e.Flags.HasFlag(ResourceFlags.Sealed))
					{
						Error = Sealed;
						return;
					}

					if(Data != null)
					{
						r.Data		= Data;
						r.Updated	= round.ConsensusTime;
						r.Flags		|= ResourceFlags.Data;
	
						if(a.SpaceReserved < a.SpaceUsed + r.Data.Value.Length)
						{
							Expand(round, a, r.Data.Value.Length);
						}
					}
					else
						r.Flags	&= ~ResourceFlags.Data;
				}

				if(Flags.HasFlag(ResourceFlags.Analysis))
				{
					if(Data.Interpretation is ReleaseAddress)
					{
						r.Flags	|= ResourceFlags.Analysis;
	
						r.AnalysisPayment	= AnalysisPayment;
						r.AnalysisConsil	= (byte)round.Analyzers.Count;
						r.AnalysisResults	= null;
	
						var s = Affect(round, Signer);
						s.Balance -= AnalysisPayment;
					} 
					else
					{
						Error = NotRelease;
						return;
					}
				}

				if(Changes.HasFlag(ResourceChanges.Recursive))
				{
					foreach(var i in r.Resources)
					{
						execute(a.Resources[i].Address);
					}
				} 
			}

			execute(Resource);
		}
	}
}