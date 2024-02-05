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
		public ResourceData			Data { get; set; }
		public string				Parent { get; set; }

		public override bool		Valid=>	(Flags & ResourceFlags.Unchangables) == 0
											//&& (!Changes.HasFlag(ResourceChanges.Years) || Changes.HasFlag(ResourceChanges.Years) && Mcv.EntityAllocationYearsMin <= Years && Years <= Mcv.EntityAllocationYearsMax)
											&& (!Changes.HasFlag(ResourceChanges.Data) || (Data == null || Data.Value.Length <= Net.Resource.DataLengthMax));
		public override string		Description => $"{Resource}, [{Changes}], [{Flags}], Years={Years}, {(Parent == null ? null : ", Parent=" + Parent)}{(Data == null ? null : $", Data={{{Data}}}")}";

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
			Changes |= ResourceChanges.Flags;
		}

		public void Change(ResourceData data)
		{
			Data = data;
			Changes |= ResourceChanges.Data;

			if(data != null)
				Changes |= ResourceChanges.NonEmtpyData;
		}

		public void Change(string parent)
		{
			Parent = parent;
			Changes |= ResourceChanges.Parent;
		}

		public void ChangeRecursive()
		{
			Changes |= ResourceChanges.Recursive;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Changes = (ResourceChanges)reader.ReadByte();
			
			if(Changes.HasFlag(ResourceChanges.Flags))													Flags = (ResourceFlags)reader.ReadByte();
			if(Changes.HasFlag(ResourceChanges.Data) && Changes.HasFlag(ResourceChanges.NonEmtpyData))	Data = reader.Read<ResourceData>();
			if(Changes.HasFlag(ResourceChanges.Parent))													Parent = reader.ReadUtf8();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Changes);

			if(Changes.HasFlag(ResourceChanges.Flags))													writer.Write((byte)Flags);
			if(Changes.HasFlag(ResourceChanges.Data) && Changes.HasFlag(ResourceChanges.NonEmtpyData))	writer.Write(Data);
			if(Changes.HasFlag(ResourceChanges.Parent))													writer.WriteUtf8(Parent);
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

			if(Author.IsExpired(aa, round.ConfirmedTime))
			{
				Error = Expired;
				return;
			}

			if(Changes.HasFlag(ResourceChanges.Data) && e.Flags.HasFlag(ResourceFlags.Sealed))
			{
				Error = CantChangeSealedResource;
				return;
			}

			var a = Affect(round, Resource.Author);
			
			void execute(ResourceAddress resource, bool ignore_renewal_errors)
			{
				var r = a.AffectResource(resource);
	
// 				if(Changes.HasFlag(ResourceChanges.Years))
// 				{
// 					if(!ignore_renewal_errors)
// 					{
// 						if(round.ConfirmedTime < r.RenewalBegin)
// 						{
// 							Error = "Renewal is allowed during last year only";
// 							return;
// 						}
// 
// 						if(round.ConfirmedTime > r.Expiration)
// 						{
// 							Error = "Expired";
// 							return;
// 						}
// 					}
// 					
// 					r.Expiration += Time.FromYears(Years);
// 					r.LastRenewalYears = Years;
// 
// 					PayForEnity(round, Years);
// 			
// 					if(e.Data != null)
// 						PayForResourceData(round, e.Data.Length, Years);
// 				}
	
				if(Changes.HasFlag(ResourceChanges.Flags))
				{
					r.Flags = r.Flags & ResourceFlags.Unchangables | Flags & ~ResourceFlags.Unchangables;
				}
	
				if(Changes.HasFlag(ResourceChanges.Parent))
				{
					if(e.Flags.HasFlag(ResourceFlags.Child)) /// remove from existing parent
					{
						var p = a.Resources.First(i => i.Resources.Contains(e.Id.Ri));
						p = a.AffectResource(p.Address);
						p.Resources = p.Resources.Where(i => i != e.Id.Ri).ToArray();
					} 
									
					if(Parent == null)
					{
						r.Flags &= ~ResourceFlags.Child;
					}
					else
					{
						var i = Array.FindIndex(a.Resources, i => i.Address.Resource == Parent);

						if(i == -1)
						{
							Error = NotFound;
							return;
						}

						r.Flags |= ResourceFlags.Child;
						var p = a.AffectResource(new ResourceAddress(a.Name, Parent));
						p.Resources = p.Resources.Append(r.Id.Ri).ToArray();
					}
				}
	
				if(Changes.HasFlag(ResourceChanges.Data))
				{
					if(Data != null)
					{
						r.Flags |= ResourceFlags.Data;
						r.Data = Data;
	
						if(a.SpaceReserved < a.SpaceUsed + r.Data.Value.Length)
						{
							var y = (byte)((a.Expiration.Days - round.ConfirmedTime.Days) / 365 + 1);

							if(y < 0)
								throw new IntegrityException();

							PayForResourceData(round, a.SpaceUsed + r.Data.Value.Length - a.SpaceReserved, y);

							a.SpaceUsed		= (short)(a.SpaceUsed + r.Data.Value.Length);
							a.SpaceReserved	= a.SpaceUsed;
						}
					}
					else
						r.Flags &= ~ResourceFlags.Data;
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