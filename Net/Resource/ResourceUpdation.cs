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
		public ResourceFlags		Flags { get; set; }
		public ResourceData			Data { get; set; }

		public override bool		Valid =>	(!Flags.HasFlag(ResourceFlags.Data) || (Data == null || Data.Value.Length <= ResourceData.LengthMax));
		public override string		Description => $"{Resource}, [{Changes}], [{Flags}], {(Data == null ? null : $", Data={{{Data}}}")}";

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

		public void ChangeRecursive()
		{
			Changes |= ResourceChanges.Recursive;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceAddress>();
			Flags		= (ResourceFlags)reader.ReadByte();
			Changes		= (ResourceChanges)reader.ReadByte();
			
			if(Flags.HasFlag(ResourceFlags.Data) && Changes.HasFlag(ResourceChanges.NotNullData))	Data = reader.Read<ResourceData>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Flags);
			writer.Write((byte)Changes);

			if(Flags.HasFlag(ResourceFlags.Data) && Changes.HasFlag(ResourceChanges.NotNullData))	writer.Write(Data);
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

				r.Flags	|= Flags&ResourceFlags.Sealed;

				if(Changes.HasFlag(ResourceChanges.Recursive))
				{
					if(r.Links != null)
					{
						foreach(var i in r.Links)
						{
							if(i.Ci == a.Id.Ci && i.Ai == a.Id.Ei)
							{
								var l = mcv.Authors.FindResource(i, round.Id);

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