using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceUpdation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public ResourceChanges		Changes	{ get; set; }
		public ResourceData			Data { get; set; }

		public override bool		Valid => !Changes.HasFlag(ResourceChanges.SetData) || Data.Value.Length <= ResourceData.LengthMax;
		public override string		Description => $"{Resource}, [{Changes}], [{Changes}], {(Data == null ? null : $", Data={{{Data}}}")}";

		public ResourceUpdation()
		{
		}

		public ResourceUpdation(ResourceAddress resource)
		{
			Resource = resource;
		}

		public void Change(ResourceData data)
		{
			Data = data;
			
			if(data != null)
				Changes = ResourceChanges.SetData;
			else
				Changes = ResourceChanges.NullData;
		}

		public void Seal()
		{
			Changes |= ResourceChanges.Seal;
		}

		public void MakeRecursive()
		{
			Changes |= ResourceChanges.Recursive;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceAddress>();
			Changes		= (ResourceChanges)reader.ReadByte();
			
			if(Changes.HasFlag(ResourceChanges.SetData))	Data = reader.Read<ResourceData>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Changes);

			if(Changes.HasFlag(ResourceChanges.SetData))	writer.Write(Data);
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var rs = new HashSet<int>();

			if(Require(round, Signer, Resource, out var ae, out var e) == false)
				return;

			var a = Affect(round, Resource.Author);
			
			void execute(ResourceAddress resource)
			{
				Fee += round.ConsensusExeunitFee;

				var r = a.AffectResource(resource.Resource);
	
				if(rs.Contains(r.Id.Ri))
					return;
				else
					rs.Add(r.Id.Ri);

				if(Changes.HasFlag(ResourceChanges.SetData))
				{
					if(e.Flags.HasFlag(ResourceFlags.Sealed))
					{
						Error = Sealed;
						return;
					}

					r.Flags		|= ResourceFlags.Data;
					r.Data		= Data;
					r.Updated	= round.ConsensusTime;
	
					if(a.SpaceReserved < a.SpaceUsed + r.Data.Value.Length)
					{
						Expand(round, a, r.Data.Value.Length);
					}
				}

				if(Changes.HasFlag(ResourceChanges.NullData))
				{
					if(e.Flags.HasFlag(ResourceFlags.Sealed))
					{
						Error = Sealed;
						return;
					}

					r.Flags	&= ~ResourceFlags.Data;
				}

				if(Changes.HasFlag(ResourceChanges.Seal))
				{
					if(e.Flags.HasFlag(ResourceFlags.Sealed))
					{
						Error = Sealed;
						return;
					}

					r.Flags	|= ResourceFlags.Sealed;

					PayForEntity(round, Time.FromYears(10));
				}

				if(Changes.HasFlag(ResourceChanges.Recursive))
				{
					if(r.Outbounds != null)
					{
						foreach(var i in r.Outbounds)
						{
							if(i.Destination.Ci.SequenceEqual(a.Id.Ci) && i.Destination.Ai == a.Id.Ei)
							{
								var l = mcv.Authors.FindResource(i.Destination, round.Id);

								execute(l.Address);
							}
						}
					}
				} 
			}

			execute(Resource);
		}
	}
}